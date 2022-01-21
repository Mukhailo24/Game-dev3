using System.Collections;
using System.Collections.Generic;
using Assets.Utility;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    public const int MAX_PLAYERS = 4;
        public const int MIN_PLAYERS = 2;
        public const int GAME_BOARD_SIZE = 9;
        public const int TOTAL_WALLS = 20;
        public Player[] playerStatus;
        public Tile[,] gameBoardStatus;
        public WallSlot[,] wallSlotStatus;
        public int[,] accessible;

        public GameBoard()
        {
            gameBoardStatus = new Tile[GAME_BOARD_SIZE, GAME_BOARD_SIZE];
            wallSlotStatus = new WallSlot[GAME_BOARD_SIZE - 1, GAME_BOARD_SIZE - 1];
            accessible = new int[GAME_BOARD_SIZE, GAME_BOARD_SIZE];
            playerStatus = new Player[2];
        }
        public bool CanPlaceHorizontalWall(int xPos, int yPos)
        {
            if (!gameBoardStatus[xPos, yPos].hasBotWall &&
                !gameBoardStatus[xPos, yPos + 1].hasBotWall &&
                 wallSlotStatus[xPos, yPos].isOpen)
            {
				bool retVal;
				PlaceHorizontalWall(this, -1, xPos, yPos);

                if (CheckWinnable(0) && CheckWinnable(1))
                {
					retVal = true;
                }
                else
                {
					retVal = false;
                }
                UndoPlaceHorizontalWall (this, -1, xPos, yPos);
				return retVal;
            }
            else
            {
                return false;
            }

        }
        public bool CanPlaceVerticalWall(int xPos, int yPos)
        {
            if (!gameBoardStatus[xPos, yPos].hasRightWall &&
                !gameBoardStatus[xPos + 1, yPos].hasRightWall &&
                wallSlotStatus[xPos, yPos].isOpen)
            {
				GameBoard.PlaceVerticalWall(this, -1, xPos, yPos);
				bool retVal;

                if (CheckWinnable(0) && CheckWinnable(1))
                {
                    retVal = true;
                }
                else
                {
					retVal = false;
                }
				UndoPlaceVerticalWall (this, -1, xPos, yPos);
				return retVal;
            }
            else
            {
                return false;
            }

        }
        public static void UndoPlaceHorizontalWall(GameBoard gameBoard, int player, int x, int y)
        {
            gameBoard.wallSlotStatus[x, y].isOpen = true;
            gameBoard.gameBoardStatus[x, y].hasBotWall = false;
            gameBoard.gameBoardStatus[x, y + 1].hasBotWall = false;
			if (player != -1)
				gameBoard.playerStatus[player].wallsLeft++;
        }
        public static void PlaceHorizontalWall(GameBoard gameBoard, int player, int x, int y)
        {
            gameBoard.wallSlotStatus[x, y].isOpen = false;
            gameBoard.gameBoardStatus[x, y].hasBotWall = true;
            gameBoard.gameBoardStatus[x, y + 1].hasBotWall = true;
			if (player != -1)
				gameBoard.playerStatus[player].wallsLeft--;
        }
        public static void PlaceVerticalWall(GameBoard gameBoard, int player, int x, int y)
        {
            gameBoard.wallSlotStatus[x, y].isOpen = false;
            gameBoard.gameBoardStatus[x, y].hasRightWall = true;
            gameBoard.gameBoardStatus[x + 1, y].hasRightWall = true;
			if (player != -1)
            	gameBoard.playerStatus[player].wallsLeft--;
        }

        public static void UndoPlaceVerticalWall(GameBoard gameBoard, int player, int x, int y)
        {
            gameBoard.wallSlotStatus[x, y].isOpen = true;
            gameBoard.gameBoardStatus[x, y].hasRightWall = false;
            gameBoard.gameBoardStatus[x + 1, y].hasRightWall = false;
			if (player != -1)
            	gameBoard.playerStatus[player].wallsLeft++;
        }
        public bool CheckWinnable(int id)
        {
			return AStarSearch.FindShortestPathLength(this, id) != -1;
        }
        public static void MovePawn(GameBoard gameBoard, int player, int newX, int newY)
        {
            gameBoard.gameBoardStatus[gameBoard.playerStatus[player].x, gameBoard.playerStatus[player].y].isOpen = true;
            gameBoard.gameBoardStatus[newX, newY].isOpen = false;
            gameBoard.playerStatus[player].x = newX;
            gameBoard.playerStatus[player].y = newY;
        }

        public bool IsPawnMoveLegal(int formerX, int formerY, int newX, int newY)
        {
            int xDiff = newX - formerX;
            int yDiff = newY - formerY;

            // DOWN
            // going down by 1 (checks on location and for walls)
            if (xDiff == 1 && yDiff == 0 &&
                formerX < 8 &&
                !gameBoardStatus[formerX, formerY].hasBotWall &&
                gameBoardStatus[formerX + 1, formerY].isOpen)
            {
                return true;
            }
            // going down jumping over 1 player directly
            else if (xDiff == 2 && yDiff == 0 &&
                    formerX < 7 &&
                    !gameBoardStatus[formerX, formerY].hasBotWall &&
                    !gameBoardStatus[formerX + 1, formerY].isOpen &&
                    !gameBoardStatus[formerX + 1, formerY].hasBotWall)
            {
                return true;
            }
            // going Down by 1 and to the Left by 1
            else if ((xDiff == 1 && yDiff == -1) &&
                    (formerX < 8 && formerY > 0) &&
                    ((formerX != 7 &&
                    !gameBoardStatus[formerX, formerY].hasBotWall &&    
                     !gameBoardStatus[formerX + 1, formerY].isOpen &&
                    !gameBoardStatus[newX, newY].hasRightWall)
                    ||
                    (formerX != 7 &&                                //going down then left
                    !gameBoardStatus[formerX, formerY].hasBotWall &&
                     gameBoardStatus[formerX + 1, formerY].hasBotWall &&
                     !gameBoardStatus[formerX + 1, formerY].isOpen &&
                    !gameBoardStatus[newX, newY].hasRightWall)
                    ||
                    (formerY == 1 &&
                    !gameBoardStatus[formerX, formerY - 1].hasRightWall &&
                    !gameBoardStatus[formerX, formerY - 1].isOpen &&
                   !gameBoardStatus[formerX, formerY - 1].hasBotWall)
                    ||
                  (formerY != 1 &&
                  !gameBoardStatus[formerX, formerY - 1].hasRightWall && //going left then down
                    gameBoardStatus[formerX, formerY - 2].hasRightWall &&
                   !gameBoardStatus[formerX, formerY - 1].isOpen &&
                   !gameBoardStatus[formerX, formerY - 1].hasBotWall)))
            {
                return true;
            }
        // UP
            // going up by 1 (checks on location and for walls)
            else if (xDiff == -1 && yDiff == 0 &&
                    formerX > 0 &&
                    !gameBoardStatus[formerX - 1, formerY].hasBotWall &&
                    gameBoardStatus[formerX - 1, formerY].isOpen)
            {
                return true;
            }
            // going up jumping over 1 player directly
            else if (xDiff == -2 && yDiff == 0 &&
                formerX > 1 &&
             !gameBoardStatus[formerX - 1, formerY].hasBotWall &&
             !gameBoardStatus[formerX - 1, formerY].isOpen &&
             !gameBoardStatus[formerX - 2, formerY].hasBotWall)
            {
                return true;
            }
            // going Up by 1 and Right by 1
            else if (xDiff == -1 && yDiff == 1 &&
                formerX > 0 && formerY < 8 &&
                ((formerX == 1 &&                               
                !gameBoardStatus[formerX - 1, formerY].hasBotWall &&
                !gameBoardStatus[formerX - 1, formerY].isOpen &&
                !gameBoardStatus[formerX - 1, formerY].hasRightWall)
                ||
                (formerX != 1 &&
                !gameBoardStatus[formerX - 1, formerY].hasBotWall && //going up then Right
                   gameBoardStatus[formerX - 2, formerY].hasBotWall &&
                  !gameBoardStatus[formerX - 1, formerY].isOpen &&
                  !gameBoardStatus[formerX - 1, formerY].hasRightWall)
                  ||
                  (formerY == 7 &&                             
                  !gameBoardStatus[formerX, formerY].hasRightWall &&
                  !gameBoardStatus[formerX, formerY + 1].isOpen &&
                  !gameBoardStatus[formerX - 1, formerY + 1].hasBotWall
                  )
                  ||
                 (formerY != 7 &&
                 !gameBoardStatus[formerX, formerY].hasRightWall && //going Right then Up
                   gameBoardStatus[formerX, formerY + 1].hasRightWall &&
                  !gameBoardStatus[formerX, formerY + 1].isOpen &&
                   !gameBoardStatus[formerX - 1, formerY + 1].hasBotWall)))
            {
                return true;

            }

            // LEFT
            // going Left by 1 (checks on location and for walls)
            else if (xDiff == 0 && yDiff == -1 &&
                    formerY > 0 &&
                    !gameBoardStatus[formerX, formerY - 1].hasRightWall &&
                    gameBoardStatus[formerX, formerY - 1].isOpen)
            {
                return true;
            }
            // going Left jumping over 1 player directly
            else if (xDiff == 0 && yDiff == -2 &&
                     formerY > 1 &&
                     !gameBoardStatus[formerX, formerY - 1].hasRightWall &&
                     !gameBoardStatus[formerX, formerY - 1].isOpen &&
                     !gameBoardStatus[formerX, formerY - 2].hasRightWall)
            {
                return true;
            }
            //going Left by 1 and Up by 1
            else if (xDiff == -1 && yDiff == -1 &&
                (formerX > 0 && formerY > 0) &&
                ((formerY == 1 &&                       
                !gameBoardStatus[formerX, formerY - 1].hasRightWall &&
                !gameBoardStatus[formerX, formerY - 1].isOpen &&
                !gameBoardStatus[newX, newY].hasBotWall)
                ||
                (formerY != 1 &&
                !gameBoardStatus[formerX, formerY - 1].hasRightWall && //going left then up
                gameBoardStatus[formerX, formerY - 2].hasRightWall &&
                !gameBoardStatus[formerX, formerY - 1].isOpen &&
                !gameBoardStatus[newX, newY].hasBotWall)
                ||
                (formerX == 1 &&                                    //special case of up then left
                !gameBoardStatus[formerX - 1, formerY].hasBotWall &&
                !gameBoardStatus[formerX - 1, formerY].isOpen &&
                !gameBoardStatus[newX, newY].hasRightWall)
                ||
                (formerX != 1 &&
                !gameBoardStatus[formerX - 1, formerY].hasBotWall && //going up then left
                gameBoardStatus[formerX - 2, formerY].hasBotWall &&
                !gameBoardStatus[formerX - 1, formerY].isOpen &&
                !gameBoardStatus[newX, newY].hasRightWall)))
            {
                return true;

            }

            // RIGHT
            // going Right by 1 (checks on location and for walls)
            else if (xDiff == 0 && yDiff == 1 &&
                formerY < 8 &&
                !gameBoardStatus[formerX, formerY].hasRightWall &&
                gameBoardStatus[formerX, formerY + 1].isOpen)
            {
                return true;
            }
            else if (xDiff == 0 && yDiff == 2 &&
                formerY < 7 &&
                !gameBoardStatus[formerX, formerY].hasRightWall &&
                !gameBoardStatus[formerX, formerY + 1].isOpen &&
                 !gameBoardStatus[formerX, formerY + 1].hasRightWall)
            {
                return true;
            }
            // going Right by 1 and Down by 1
            else if (xDiff == 1 && yDiff == 1 &&
                (formerX < 8 && formerY < 8) &&
             ((formerY == 7 &&       
             !gameBoardStatus[formerX, formerY].hasRightWall &&
             !gameBoardStatus[formerX, formerY + 1].isOpen &&
             !gameBoardStatus[formerX, formerY + 1].hasBotWall)
             ||
             (formerY != 7 &&
             !gameBoardStatus[formerX, formerY].hasRightWall && //going Right then Down
                gameBoardStatus[formerX, formerY + 1].hasRightWall &&
               !gameBoardStatus[formerX, formerY + 1].isOpen &&
               !gameBoardStatus[formerX, formerY + 1].hasBotWall)
               ||
               (formerX == 7 &&                       
               !gameBoardStatus[formerX, formerY].hasBotWall &&
               !gameBoardStatus[formerX + 1, formerY].isOpen &&
               !gameBoardStatus[formerX + 1, formerY].hasRightWall)
               ||
              (!gameBoardStatus[formerX, formerY].hasBotWall && //going Down then Right
                gameBoardStatus[formerX + 1, formerY].hasBotWall &&
               !gameBoardStatus[formerX + 1, formerY].isOpen &&
               !gameBoardStatus[formerX + 1, formerY].hasRightWall)))
            {
                return true;
            }
            return false;
        }
        bool CanMoveDown(int posX, int posY) 
        {
            if (!gameBoardStatus[posX, posY].hasBotWall)
                return true;
            else
                return false;
        }

        bool CanMoveUp(int posX, int posY)
        {
            if (!gameBoardStatus[posX - 1, posY].hasBotWall)
                return true;
            else
                return false;
        }

        bool CanMoveRight(int posX, int posY)
        {
            if (!gameBoardStatus[posX, posY].hasRightWall)
                return true;
            else
                return false;
        }

        bool CanMoveLeft(int posX, int posY)
        {
            if (!gameBoardStatus[posX, posY - 1].hasRightWall)
                return true;
            else
                return false;
        }
        public bool PawnCanMoveSimple(int formerX, int formerY, int newX, int newY)
        {
            int diffX = newX - formerX;
            int diffY = newY - formerY;
            if (diffX == 1 && diffY == 0)
                return CanMoveDown(formerX, formerY);
            else if (diffX == -1 && diffY == 0)
                return CanMoveUp(formerX, formerY);
            else if (diffX == 0 && diffY == -1)
                return CanMoveLeft(formerX, formerY);
            else if (diffX == 0 && diffY == 1)
                return CanMoveRight(formerX, formerY);
            else
                return false;
        }
}
