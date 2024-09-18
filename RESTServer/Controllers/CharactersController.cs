using Microsoft.AspNetCore.Mvc;

namespace RESTServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CharactersController : Controller
    {
        public class Character
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Damage { get; set; }
            public int Health { get; set; }
            public int Level { get; set; }
        }

        private static Dictionary<int, Character> _characters;

        static CharactersController()
        {
            _characters = new Dictionary<int, Character>();
            _characters.Add(0, new Character()
            {
                Id = 0,
                Name = "Alice",
                Damage = 10,
                Health = 150,
                Level = 4,
            });
            _characters.Add(1, new Character()
            {
                Id = 1,
                Name = "Bob",
                Damage = 25,
                Health = 80,
                Level = 6,
            });
            _characters.Add(2, new Character()
            {
                Id = 2,
                Name = "Charlie",
                Damage = 5,
                Health = 100,
                Level = 2,
            });
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_characters.Values.ToArray());
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (_characters.ContainsKey(id))
                return Ok(_characters[id]);
            return NotFound();
        }

        // TODO: Implement endpoint to POST a new Character to the list

        // TODO: Implement endpoint to DELETE a specific Character from the list
    }
}
