using ChessChallenge.API;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] legalMoves = board.GetLegalMoves();
        Move bestMove = legalMoves[0];

        int maxEval = int.MinValue;

        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);

            int eval = -Search(board, 3);
            if (eval > maxEval)
            {
                maxEval = eval;
                bestMove = move;
            }

            board.UndoMove(move);
        }

        return bestMove;
    }

    int Search(Board board, int depth)
    {
        if (depth == 0 || board.IsInCheckmate() || board.IsDraw())
            // Return score relative to side to move
            return (board.IsWhiteToMove ? 1 : -1) * Evaluate(board);

        int maxEval = int.MinValue;

        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);

            int eval = -Search(board, depth - 1);
            if (eval > maxEval)
                maxEval = eval;

            board.UndoMove(move);
        }

        return maxEval;
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
