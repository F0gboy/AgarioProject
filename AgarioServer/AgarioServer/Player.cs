using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarioServer
{
    class Player
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public float X { get; set; } // Position X
        public float Y { get; set; } // Position Y
        public float Radius { get; set; } // Size/Radius
        public float Speed { get; set; } // Movement speed

        public Player(Guid id, string name)
        {
            Id = id;
            Name = name;
            X = 0;
            Y = 0;
            Radius = 10; // starting size
            Speed = 1.0f;
        }
    }
}
