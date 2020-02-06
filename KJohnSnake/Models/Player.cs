namespace KJohnSnake.Models
{
    using System;
    using System.Collections.Generic;
    [Serializable]
    public class Player
    {
        public string Name { get; set; }
        public int Score { get; set; }
        public int Time { get; set; }
        public int Speed { get; set; }
        public char MovingDirection { get; set; }
        public List<Coordinates> Coordinates { get; set; }
        public Coordinates FoodPosition { get; set; }
    }
}