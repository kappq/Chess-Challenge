using ChessChallenge.API;

public class MyBot : IChessBot
{
    const int Infinity = int.MaxValue;

    int? moveTime;
    Timer? moveTimer;

    public Move Think(Board board, Timer timer)
    {
        // Use 2.25% of the available time
        moveTime = timer.MillisecondsRemaining / 40;
        moveTimer = timer;

        Move[] legalMoves = GetSortedMoves(board);
        Move bestMove = legalMoves[0];

        int maxScore = -Infinity;

        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);
            int score = -Search(board, 10, -Infinity, Infinity);
            board.UndoMove(move);

            if (score > maxScore)
            {
                maxScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    int Search(Board board, int depth, int alpha, int beta)
    {
        if (depth == 0 || board.IsInCheckmate() || board.IsDraw() || moveTimer?.MillisecondsElapsedThisTurn > moveTime)
            return Quiesce(board, alpha, beta);

        foreach (Move move in GetSortedMoves(board))
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

        foreach (Move move in board.GetLegalMoves())
        {
            if (!move.IsCapture)
                continue;

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

    Move[] GetSortedMoves(Board board)
    {
        Move[] moves = board.GetLegalMoves();

        for (int i = 1, j = 0; i < moves.Length; i++)
        {
            Move move = moves[i];

            board.MakeMove(move);
            bool moveIsCheck = board.IsInCheck();
            board.UndoMove(move);

            if (move.IsCapture || move.IsPromotion || moveIsCheck)
            {
                Move temp = moves[j];
                moves[j++] = moves[i];
                moves[i] = temp;
            }
        }

        return moves;
    }

    int Evaluate(Board board)
    {
        return 200 * (board.GetPieceList(PieceType.King, true).Count - board.GetPieceList(PieceType.King, false).Count) +
            9 * (board.GetPieceList(PieceType.Queen, true).Count - board.GetPieceList(PieceType.Queen, false).Count) +
            5 * (board.GetPieceList(PieceType.Rook, true).Count - board.GetPieceList(PieceType.Rook, false).Count) +
            3 * (board.GetPieceList(PieceType.Bishop, true).Count - board.GetPieceList(PieceType.Bishop, false).Count) +
            3 * (board.GetPieceList(PieceType.Knight, true).Count - board.GetPieceList(PieceType.Knight, false).Count) +
            board.GetPieceList(PieceType.Pawn, true).Count - board.GetPieceList(PieceType.Pawn, false).Count;
    }
}
