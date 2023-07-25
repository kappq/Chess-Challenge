using ChessChallenge.API;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        bool maximizing = board.IsWhiteToMove;
        Move[] legalMoves = board.GetLegalMoves();

        if (maximizing)
        {
            int maxEval = int.MinValue;
            Move bestMove = legalMoves[0];

            foreach (Move move in legalMoves)
            {
                board.MakeMove(move);

                int eval = Search(board, 3, false);
                if (eval > maxEval)
                {
                    maxEval = eval;
                    bestMove = move;
                }

                board.UndoMove(move);
            }

            return bestMove;
        }
        else
        {
            int minEval = int.MaxValue;
            Move bestMove = legalMoves[0];

            foreach (Move move in legalMoves)
            {
                board.MakeMove(move);

                int eval = Search(board, 3, true);
                if (eval < minEval)
                {
                    minEval = eval;
                    bestMove = move;
                }

                board.UndoMove(move);
            }

            return bestMove;
        }
    }

    int Search(Board board, int depth, bool maximizing)
    {
        if (depth == 0 || board.IsInCheckmate() || board.IsDraw())
            return Evaluate(board);

        if (maximizing)
        {
            int maxEval = int.MinValue;

            foreach (Move move in board.GetLegalMoves())
            {
                board.MakeMove(move);

                int eval = Search(board, depth - 1, false);
                maxEval = System.Math.Max(maxEval, eval);

                board.UndoMove(move);
            }

            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;

            foreach (Move move in board.GetLegalMoves())
            {
                board.MakeMove(move);

                int eval = Search(board, depth - 1, true);
                minEval = System.Math.Min(minEval, eval);

                board.UndoMove(move);
            }

            return minEval;
        }
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
