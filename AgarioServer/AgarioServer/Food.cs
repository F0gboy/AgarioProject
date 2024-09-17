using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarioServer
{
    class Food
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Size { get; set; }

        public Food(float x, float y, float size)
        {
            X = x;
            Y = y;
            Size = size;
        }
    }

}
