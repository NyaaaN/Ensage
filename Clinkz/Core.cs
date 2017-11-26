
using Ensage.Common.Extensions;

namespace Clinkz
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using Ensage;
    using Ensage.Common.Threading;
    using Ensage.SDK.Extensions;
    using Ensage.SDK.Handlers;
    using Ensage.SDK.Helpers;
    using Ensage.SDK.Abilities.Items;
    using Ensage.SDK.Abilities.npc_dota_hero_clinkz;
    using Ensage.SDK.Inventory.Metadata;
    using Ensage.SDK.Orbwalker.Modes;
    using Ensage.SDK.Renderer.Particle;
    using Ensage.SDK.Service;
    using Ensage.SDK.TargetSelector;

    using log4net;

    using PlaySharp.Toolkit.Logging;

    using SharpDX;

    internal class Core : KeyPressOrbwalkingModeAsync
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IParticleManager _particleManager;
        private readonly Configuration _configuration;
        private readonly IUpdateHandler _targetParticleUpdateHandler;
        private readonly ITargetSelectorManager _targetSelector;

    

        private Unit _target;


        public Core(IServiceContext context, Configuration configuration) : base(context, configuration.ComboKey)
        { 
            _targetSelector = context.TargetSelector;
            _configuration = configuration;
            
            _particleManager = Context.Particle;
            _targetParticleUpdateHandler = UpdateManager.Subscribe(UpdateTargetParticle, 0, false);
        }


        

        [ItemBinding]
        public item_mjollnir Mjollnir { get; set; }
        
        [ItemBinding]
        public item_nullifier Nullifier { get; set; }
        
        [ItemBinding]
        public item_bloodthorn Bloodthorn { get; set; }

        [ItemBinding]
        public item_orchid Orchid { get; set; }
             
        [ItemBinding]
        public item_sheepstick SheepStick { get; set; }

        [ItemBinding]
        public item_solar_crest SolarCrest { get; set; }

        [ItemBinding]
        public item_diffusal_blade DiffusalBlade { get; set; }

        [ItemBinding]
        public item_medallion_of_courage Meddallion { get; set; }


        public clinkz_strafe Strafe { get; set; }
        public clinkz_searing_arrows Arrows { get; set; }
        public clinkz_death_pact DeathPact { get; set; }


        public override async Task ExecuteAsync(CancellationToken token)
        {
            try
            {


                _target = _targetSelector.Active.GetTargets().FirstOrDefault();




                var unitMaxHp = EntityManager<Unit>.Entities.Where(x =>
                    x.IsValid &&
                    x.IsVisible &&
                    x.IsAlive &&
                    x.IsSpawned &&
                    x.Distance2D(Owner) < 400 &&
                    (x.NetworkName == "CDOTA_BaseNPC_Creep_Neutral" ||
                     x.NetworkName == "CDOTA_BaseNPC_Creep" ||
                     x.NetworkName == "CDOTA_BaseNPC_Creep_Lane" || 
                     x.NetworkName == "CDOTA_BaseNPC_Creep_Siege")).OrderBy(x => x.MaximumHealth).LastOrDefault();

                if (_target == null || _target.IsAttackImmune() || _target.IsInvulnerable())
                {
                    Orbwalker.OrbwalkTo(null);
                    return;
                }


                if (_configuration.AbilityManager.Value.IsEnabled(DeathPact.Ability.Name) && DeathPact.CanBeCasted && !_target.IsEthereal() &&
                    unitMaxHp != null)
                {

                    DeathPact.UseAbility(unitMaxHp);
                    await Await.Delay(DeathPact.GetCastDelay(unitMaxHp), token);
                }


                if (_configuration.AbilityManager.Value.IsEnabled(Strafe.Ability.Name) && Strafe.CanBeCasted && !_target.IsEthereal() &&
                    Owner.IsInAttackRange(_target))
                {
                    Strafe.UseAbility();
                    await Await.Delay(Strafe.GetCastDelay(), token);
                }

                if (_configuration.AbilityManager.Value.IsEnabled(Arrows.Ability.Name) && Arrows.CanBeCasted && !_target.IsEthereal() &&
                    !Arrows.Ability.IsAutoCastEnabled && Owner.IsInAttackRange(_target))
                {
                    Arrows.Ability.ToggleAutocastAbility();
                }


                if (SolarCrest != null && SolarCrest.CanBeCasted && !Owner.IsAttacking() && !_target.IsEthereal() &&
                    _configuration.ItemManager.Value.IsEnabled(SolarCrest.Ability.Name) && !_target.IsMagicImmune() &&
                    Owner.IsInAttackRange(_target))
                {
                    SolarCrest.UseAbility(_target);
                    await Await.Delay(SolarCrest.GetCastDelay(_target), token);
                }

                if (Meddallion != null && Meddallion.CanBeCasted && !Owner.IsAttacking() && !_target.IsEthereal() &&
                    _configuration.ItemManager.Value.IsEnabled(Meddallion.Ability.Name) && !_target.IsMagicImmune() &&
                    Owner.IsInAttackRange(_target))
                {
                    Meddallion.UseAbility(_target);
                    await Await.Delay(Meddallion.GetCastDelay(_target), token);
                }

               
                if (DiffusalBlade != null && DiffusalBlade.CanBeCasted  && !Owner.IsAttacking() && !_target.IsEthereal() &&
                    _configuration.ItemManager.Value.IsEnabled(DiffusalBlade.Ability.Name) && !_target.IsMagicImmune() &&
                    Owner.IsInAttackRange(_target))
                {
                    DiffusalBlade.Ability.UseAbility(_target);
                    await Await.Delay(DiffusalBlade.GetCastDelay(_target), token);

                }
  
                    if (Mjollnir != null && Mjollnir.CanBeCasted && !Owner.IsAttacking() &&
                    Owner.IsInAttackRange(_target) && _configuration.ItemManager.Value.IsEnabled(Mjollnir.Ability.Name))
                {
                    Mjollnir.UseAbility(Owner);
                    await Await.Delay(Mjollnir.GetCastDelay(Owner), token);
                }

                if (Nullifier != null && Nullifier.CanBeCasted && !Owner.IsAttacking() && !_target.IsMagicImmune() &&
                    _configuration.ItemManager.Value.IsEnabled(Nullifier.ToString()) && Owner.IsInAttackRange(_target))
                {
                    Nullifier.UseAbility(_target);
                    await Await.Delay(Nullifier.GetCastDelay(_target), token);
                }
                
                if (Bloodthorn != null && Bloodthorn.CanBeCasted && !Owner.IsAttacking() && !_target.IsMagicImmune() &&
                    _configuration.ItemManager.Value.IsEnabled(Bloodthorn.ToString()) && Owner.IsInAttackRange(_target))
                {
                    Bloodthorn.UseAbility(_target);
                    await Await.Delay(Bloodthorn.GetCastDelay(_target), token);
                }

                if (SheepStick != null && SheepStick.CanBeCasted && !Owner.IsAttacking() && !_target.IsMagicImmune() &&
                    _configuration.ItemManager.Value.IsEnabled(SheepStick.Ability.Name) &&
                    Owner.IsInAttackRange(_target))
                {
                    SheepStick.UseAbility(_target);
                    await Await.Delay(SheepStick.GetCastDelay(_target), token);
                }

                if (Orchid != null && Orchid.CanBeCasted && !Owner.IsAttacking() && !_target.IsMagicImmune() &&
                    _configuration.ItemManager.Value.IsEnabled(Orchid.Ability.Name) && Owner.IsInAttackRange(_target))
                {
                    Orchid.UseAbility(_target);
                    await Await.Delay(Orchid.GetCastDelay(_target), token);
                }


            if (_target != null && (_target.IsInvulnerable() || _target.IsAttackImmune()))
            {
                Orbwalker.Move(Game.MousePosition);
            }
            else if (_target != null)
            {
                Orbwalker.OrbwalkTo(_target);
            }
            else
            {
                Orbwalker.Move(Game.MousePosition);
            }

        }
        catch (Exception e)
            {
                Log.Error(e);
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            Context.Inventory.Attach(this);
            
            MenuKey.PropertyChanged += MenuKeyOnPropertyChanged;

            Arrows = Context.AbilityFactory.GetAbility<clinkz_searing_arrows>();
            Strafe = Context.AbilityFactory.GetAbility<clinkz_strafe>();
            DeathPact = Context.AbilityFactory.GetAbility<clinkz_death_pact>();
        }

        protected override void OnDeactivate()
        {
            Context.Inventory.Detach(this);
          
            MenuKey.PropertyChanged -= MenuKeyOnPropertyChanged;
            UpdateManager.Unsubscribe(UpdateTargetParticle);
            base.OnDeactivate();
        }

        private void MenuKeyOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (MenuKey)
            {
                if (_configuration.DrawTargetParticle)
                {
                    _targetParticleUpdateHandler.IsEnabled = true;
                }
            }
            else
            {
                if (_configuration.DrawTargetParticle)
                {
                    _particleManager.Remove("Target");
                    _targetParticleUpdateHandler.IsEnabled = false;
                }
            }
        }
        private void UpdateTargetParticle()
        {
            if (_target == null || !_target.IsValid || !_target.IsVisible)
            {
                _particleManager.Remove("Target");
                return;
            }
            _particleManager.DrawTargetLine(Owner, "Target", _target.Position, Color.DeepPink);
        }
    }
}
