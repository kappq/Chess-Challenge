using ChessChallenge.API;
using System.Linq;

public class MyBot : IChessBot
{
    const int Infinity = int.MaxValue;

    // One value for each `PieceType` (Null, Pawn, Knight, Bishop, Queen, King)
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    int? moveTime;
    Timer? moveTimer;

    public Move Think(Board board, Timer timer)
    {
        // Use 2.25% of the available time
        moveTime = timer.MillisecondsRemaining / 40;
        moveTimer = timer;

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
        if (depth == 0 || moveTimer?.MillisecondsElapsedThisTurn > moveTime)
            return Quiesce(board, alpha, beta);
        if (board.IsInCheckmate())
            return -Infinity;
        if (board.IsDraw())
            return 0;

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
                score += 10 * (pieceValues[(int)move.CapturePieceType] - pieceValues[(int)move.MovePieceType]);

            if (move.IsPromotion)
                score += pieceValues[(int)move.PromotionPieceType];

            return score;
        }).ToArray();
    }

    int Evaluate(Board board)
    {
        int score = 0;

        // Iterate over piece types ignoring `PieceType.Null`
        for (int pieceType = 1; pieceType <= 6; pieceType++)
        {
            int pieceCount = board.GetPieceList((PieceType)pieceType, true).Count - board.GetPieceList((PieceType)pieceType, false).Count;
            score += pieceValues[pieceType] * pieceCount;
        }

        return score;
    }
}
