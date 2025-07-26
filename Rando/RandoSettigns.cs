using PaleCourtCharms;

namespace PaleCourtCharms.Rando
{
    public class RandoSettings
    {
        public bool Enabled { get; set; }
        public bool RandomizeCosts { get; set; }

        public static RandoSettings FromSaveSettings(SaveModSettings s)
        {
            return new RandoSettings
            {
                Enabled = s != null && s.EnabledCharms.Count > 0,
                RandomizeCosts = PaleCourtCharms.GlobalSettings.RandomizeCosts
            };
        }


        public void ApplyTo(SaveModSettings s)
        {
            if (!Enabled)
                s.EnabledCharms.Clear();

            PaleCourtCharms.GlobalSettings.RandomizeCosts = RandomizeCosts;
        }

        public bool IsEnabled() => Enabled;
    }
}