namespace BlogApp.Core.Services
{
    public interface IGuestNameGenerator
    {
        string GenerateGuestName();
    }

    public class GuestNameGenerator : IGuestNameGenerator
    {
        private static readonly string[] Adjectives = new[]
        {
            "Happy", "Clever", "Brave", "Wise", "Swift", "Calm", "Bright", "Bold",
            "Gentle", "Keen", "Noble", "Quick", "Silent", "Witty", "Zesty", "Agile",
            "Cosmic", "Dynamic", "Epic", "Fantastic", "Grand", "Heroic", "Infinite",
            "Jolly", "Kinetic", "Legendary", "Majestic", "Nimble", "Optimal", "Perfect",
            "Quantum", "Radiant", "Stellar", "Turbo", "Ultimate", "Vibrant", "Wondrous"
        };

        private static readonly string[] Nouns = new[]
        {
            "Panda", "Tiger", "Eagle", "Dolphin", "Phoenix", "Dragon", "Wolf", "Falcon",
            "Lynx", "Hawk", "Raven", "Fox", "Bear", "Lion", "Owl", "Shark",
            "Panther", "Cobra", "Pegasus", "Griffin", "Sphinx", "Kraken", "Hydra",
            "Unicorn", "Thunder", "Storm", "Blaze", "Frost", "Shadow", "Lightning",
            "Comet", "Nova", "Nebula", "Galaxy", "Cosmos", "Meteor", "Eclipse"
        };

        private readonly Random _random = new Random();

        public string GenerateGuestName()
        {
            var adjective = Adjectives[_random.Next(Adjectives.Length)];
            var noun = Nouns[_random.Next(Nouns.Length)];
            var number = _random.Next(100, 999);

            return $"{adjective}{noun}{number}";
        }
    }
}
