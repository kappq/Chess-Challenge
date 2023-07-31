using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
    const int Infinity = 99999999;

    int[] _pieceValues = { 0, 100, 300, 300, 500, 900, 20000 };

    UInt64[,] _pieceSquareTables = {
        // Pawn
        { 0x8080808080808080, 0x858A8A6C6C8A8A85, 0x857B768080767B85, 0x8080809494808080, 0x85858A99998A8585, 0x8A8A949E9E948A8A, 0xB2B2B2B2B2B2B2B2, 0x8080808080808080 },
        // Knight
        { 0x4E5862626262584E, 0x586C808585806C58, 0x62858A8F8F8A8562, 0x62808F94948F8062, 0x62858F94948F8562, 0x62808A8F8F8A8062, 0x586C808080806C58, 0x4E5862626262584E },
        // Bishop
        { 0x6C7676767676766C, 0x7685808080808576, 0x768A8A8A8A8A8A76, 0x76808A8A8A8A8076, 0x7685858A8A858576, 0x7680858A8A858076, 0x7680808080808076, 0x6C7676767676766C },
        // Rook
        { 0x8080808585808080, 0x7B8080808080807B, 0x7B8080808080807B, 0x7B8080808080807B, 0x7B8080808080807B, 0x7B8080808080807B, 0x858A8A8A8A8A8A85, 0x8080808080808080 },
        // Queen
        { 0x6C76767B7B76766C, 0x7680808080858076, 0x7680858585858576, 0x7B80858585858080, 0x7B8085858585807B, 0x7680858585858076, 0x7680808080808076, 0x6C76767B7B76766C },
        // King middle game
        { 0x949E8A80808A9E94, 0x9494808080809494, 0x766C6C6C6C6C6C76, 0x6C6262585862626C, 0x6258584E4E585862, 0x6258584E4E585862, 0x6258584E4E585862, 0x6258584E4E585862 },
        // King end game
        { 0x4E6262626262624E, 0x6262808080806262, 0x6276949E9E947662, 0x62769EA8A89E7662, 0x62769EA8A89E7662, 0x6276949E9E947662, 0x626C768080766C62, 0x4E58626C6C62584E },
    };

    Timer? _timer;
    int _timeLimit;

    public Move Think(Board board, Timer timer)
    {
        // Use 2.25% of the available time
        _timer = timer;
        _timeLimit = timer.MillisecondsRemaining / 40;

        Move[] legalMoves = board.GetLegalMoves();
        legalMoves = OrderMoves(legalMoves);

        Move bestMove = legalMoves.MaxBy(move =>
        {
            board.MakeMove(move);
            int score = -Search(board, 4, -Infinity, Infinity);
            board.UndoMove(move);

            return score;
        });

        return bestMove;
    }

    int Search(Board board, int depth, int alpha, int beta)
    {
        if (board.IsInCheckmate())
            // Subtract the depth to incentivize earlier mates and avoid shuffling pieces
            return -Infinity - depth;
        if (board.IsRepeatedPosition())
            // Discourage draws by repetition
            return -20;
        if (board.IsDraw())
            return 0;
        if (depth == 0 || _timer?.MillisecondsElapsedThisTurn > _timeLimit)
            return Quiesce(board, alpha, beta);

        Move[] legalMoves = board.GetLegalMoves();
        legalMoves = OrderMoves(legalMoves);

        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);
            int score = -Search(board, depth - 1, -beta, -alpha);
            board.UndoMove(move);

            if (score >= beta)
                return beta;
            if (score > alpha)
                alpha = score;
        }

        return alpha;
    }

    private int Quiesce(Board board, int alpha, int beta)
    {
        // Return score relative to side to move
        int standPat = (board.IsWhiteToMove ? 1 : -1) * Evaluate(board);

        if (standPat >= beta)
            return beta;
        if (alpha < standPat)
            alpha = standPat;

        foreach (Move move in board.GetLegalMoves(capturesOnly: true))
        {
            board.MakeMove(move);
            int score = -Quiesce(board, -beta, -alpha);
            board.UndoMove(move);

            if (score >= beta)
                return beta;
            if (score > alpha)
                alpha = score;
        }

        return alpha;
    }

    Move[] OrderMoves(Move[] moves)
    {
        return moves.OrderByDescending(move =>
        {
            int score = 0;

            if (move.IsCapture)
                score += 10 * _pieceValues[(int)move.CapturePieceType] - _pieceValues[(int)move.MovePieceType];

            if (move.IsPromotion)
                score += _pieceValues[(int)move.PromotionPieceType];

            return score;
        }).ToArray();
    }

    int Evaluate(Board board)
    {
        int score = 0;

        for (int index = 0; index < 64; index++)
        {
            Square square = new Square(index);

            Piece piece = board.GetPiece(square);
            if (piece.IsNull)
                continue;

            int color = piece.IsWhite ? 1 : -1;

            score += color * _pieceValues[(int)piece.PieceType];

            UInt64 pieceSquareTable = _pieceSquareTables[(int)(piece.PieceType) - 1, piece.IsWhite ? square.Rank : 7 - square.Rank];
            int pieceSquareValue = (int)((pieceSquareTable >> (square.File * 8)) & 0xff) - 128;

            score += color * pieceSquareValue;
        }

        return score;
    }
}
