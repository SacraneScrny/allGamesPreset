using UnityEngine.Scripting;

namespace Sackrany.Actor.Traits.Affinity
{
    // --- ПЕРВООСНОВЫ ---
    [Preserve]
    public partial class Fire : AAffinity<Fire> {
        public override string Name => "Fire";
        public override void Setup() {
            _counter = new IAffinity[] { Affinity<Ice>.Instance, Affinity<Nature>.Instance };
            _counteredBy = new IAffinity[] { Affinity<Water>.Instance, Affinity<Earth>.Instance };
            OnSetup();
        }
        partial void OnSetup();
    }
    [Preserve]
    public partial class Water : AAffinity<Water> {
        public override string Name => "Water";
        public override void Setup() {
            _counter = new IAffinity[] { Affinity<Fire>.Instance };
            _counteredBy = new IAffinity[] { Affinity<Lightning>.Instance, Affinity<Ice>.Instance, Affinity<Nature>.Instance };
            OnSetup();
        }
        partial void OnSetup();
    }
    [Preserve]
    public partial class Earth : AAffinity<Earth> {
        public override string Name => "Earth";
        public override void Setup() {
            _counter = new IAffinity[] { Affinity<Lightning>.Instance, Affinity<Fire>.Instance };
            _counteredBy = new IAffinity[] { Affinity<Nature>.Instance, Affinity<Water>.Instance };
            OnSetup();
        }
        partial void OnSetup();
    }
    [Preserve]
    public partial class Wind : AAffinity<Wind> {
        public override string Name => "Wind";
        public override void Setup() {
            _counter = new IAffinity[] { Affinity<Nature>.Instance };
            _counteredBy = new IAffinity[] { Affinity<Ice>.Instance };
            OnSetup();
        }
        partial void OnSetup();
    }
    [Preserve]
    public partial class Lightning : AAffinity<Lightning> {
        public override string Name => "Lightning";
        public override void Setup() {
            _counter = new IAffinity[] { Affinity<Water>.Instance, Affinity<Ice>.Instance };
            _counteredBy = new IAffinity[] { Affinity<Earth>.Instance };
            OnSetup();
        }
        partial void OnSetup();
    }
    [Preserve]
    public partial class Ice : AAffinity<Ice> {
        public override string Name => "Ice";
        public override void Setup() {
            _counter = new IAffinity[] { Affinity<Water>.Instance, Affinity<Wind>.Instance };
            _counteredBy = new IAffinity[] { Affinity<Fire>.Instance, Affinity<Lightning>.Instance };
            OnSetup();
        }
        partial void OnSetup();
    }

    // --- ЖИЗНЬ ---
    [Preserve]
    public partial class Nature : AAffinity<Nature> {
        public override string Name => "Nature";
        public override void Setup() {
            _counter = new IAffinity[] { Affinity<Earth>.Instance, Affinity<Water>.Instance };
            _counteredBy = new IAffinity[] { Affinity<Fire>.Instance, Affinity<Wind>.Instance };
            OnSetup();
        }
        partial void OnSetup();
    }

    // --- СУДЬБА (Свет и Тьма обычно контрят друг друга взаимно) ---
    [Preserve]
    public partial class Holy : AAffinity<Holy> {
        public override string Name => "Holy";
        public override void Setup() {
            _counter = new IAffinity[] { Affinity<Shadow>.Instance };
            _counteredBy = new IAffinity[] { Affinity<Shadow>.Instance };
            OnSetup();
        }
        partial void OnSetup();
    }
    [Preserve]
    public partial class Shadow : AAffinity<Shadow> {
        public override string Name => "Shadow";
        public override void Setup() {
            _counter = new IAffinity[] { Affinity<Holy>.Instance };
            _counteredBy = new IAffinity[] { Affinity<Holy>.Instance };
            OnSetup();
        }
        partial void OnSetup();
    }

    // --- ЧИСТАЯ МАГИЯ (Нейтральна ко всему) ---
    [Preserve]
    public partial class Arcane : AAffinity<Arcane> {
        public override string Name => "Arcane";
        public override void Setup() {
            OnSetup();
        }
        partial void OnSetup();
    }
}