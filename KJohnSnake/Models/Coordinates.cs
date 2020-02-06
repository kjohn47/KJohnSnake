namespace KJohnSnake.Models
{
    using System;

    [Serializable]
    public class Coordinates
    {
        public Coordinates()
        {
            
        }
        public Coordinates(int x, int y)
        {
            this.PositionX = x;
            this.PositionY = y;
        }
        
        public int PositionX { get; set; }
        public int PositionY { get; set; }
    }
}