﻿namespace Lifestealer
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using Ensage;
    using Ensage.Common.Extensions;
    using Ensage.Common.Menu;
    using Ensage.Common.Threading;
    using Ensage.SDK.Abilities;
    using Ensage.SDK.Abilities.Items;
    using Ensage.SDK.Extensions;
    using Ensage.SDK.Inventory.Metadata;
    using Ensage.SDK.Orbwalker.Modes;
    using Ensage.SDK.Service;
    using Ensage.SDK.TargetSelector;

    using SharpDX;

    using log4net;
    using PlaySharp.Toolkit.Helper.Annotations;
    using PlaySharp.Toolkit.Logging;
    
    using UnitExtensions = Ensage.SDK.Extensions.UnitExtensions;

    [PublicAPI]
    public class Lifestealer : KeyPressOrbwalkingModeAsync
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IServiceContext context;

        private Hero EnemyHero { get; set; }

        private ITargetSelectorManager TargetSelector { get; }

        public LifestealerConfiguration Config { get; }

        public Lifestealer(Key key,LifestealerConfiguration config, IServiceContext context): base(context, key)
        {
            this.Config = config;
            this.context = context;
            this.TargetSelector = context.TargetSelector;
        }

        [ItemBinding]
        public item_blink BlinkDagger { get; private set; }

        [ItemBinding]
        public item_abyssal_blade Basher { get; private set; }

        [ItemBinding]
        public item_heavens_halberd Heaven { get; private set; }

        [ItemBinding]
        public item_bloodthorn BloodThorn { get; private set; }

        [ItemBinding]
        public item_mjollnir Mjollnir { get; private set; }

        [ItemBinding]
        public item_orchid Orchid { get; private set; }

        [ItemBinding]
        public item_diffusal_blade DiffBlade { get; private set; }


        private Ability Rage { get; set; }
        private Ability OpenWounds { get; set; }


        public override async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                var sliderValue = this.Config.UseBlinkPrediction.Item.GetValue<Slider>().Value;

                if (Config.TargetItem.Value.SelectedValue.Contains("Lock") && Context.TargetSelector.IsActive
                    && (!CanExecute || EnemyHero == null || !EnemyHero.IsValid || !EnemyHero.IsAlive))
                {
                    EnemyHero = Context.TargetSelector.Active.GetTargets().FirstOrDefault() as Hero;
                }
                else if (Config.TargetItem.Value.SelectedValue.Contains("Default") && Context.TargetSelector.IsActive)
                {
                    EnemyHero = Context.TargetSelector.Active.GetTargets().FirstOrDefault() as Hero;
                }

                var Silenced = UnitExtensions.IsSilenced(base.Owner);

                if (EnemyHero != null)
                {
                    if (this.BlinkDagger != null
                        && (this.BlinkDagger.CanBeCasted
                        && base.Owner.Distance2D(EnemyHero) <= 1200 + sliderValue
                        && !(base.Owner.Distance2D(EnemyHero) <= 400)
                        && this.Config.ItemToggler.Value.IsEnabled(this.BlinkDagger.Item.Name)))
                    {
                        var l = (this.Owner.Distance2D(EnemyHero) - sliderValue) / sliderValue;
                        var posA = this.Owner.Position;
                        var posB = EnemyHero.Position;
                        var x = (posA.X + (l * posB.X)) / (1 + l);
                        var y = (posA.Y + (l * posB.Y)) / (1 + l);
                        var position = new Vector3((int)x, (int)y, posA.Z);

                        this.BlinkDagger.UseAbility(position);
                        await Await.Delay(BlinkDagger.GetCastDelay(position), token);
                    }

                    if (!Silenced)
                    {
                        if (this.Config.AbilityToggler.Value.IsEnabled(this.Rage.Name)
                        && base.Owner.IsAttacking()
                        && this.Rage.CanBeCasted())
                        {
                            this.Rage.UseAbility();
                            await Await.Delay(this.GetAbilityDelay(base.Owner, Rage), token);
                        }

                        if (this.Config.AbilityToggler.Value.IsEnabled(this.OpenWounds.Name)
                            && this.OpenWounds.CanBeCasted(EnemyHero)
                            && this.OpenWounds.CanHit(EnemyHero))
                        {
                            this.OpenWounds.UseAbility(EnemyHero);
                            await Await.Delay(this.GetAbilityDelay(EnemyHero, OpenWounds), token);
                        }
                    }

                    if (this.Basher != null &&
                        base.Owner.IsAttacking() &&
                        this.Basher.CanBeCasted &&
                        this.Basher.CanHit(EnemyHero) &&
                        this.Config.ItemToggler.Value.IsEnabled(Basher.ToString()))
                    {
                        this.Basher.UseAbility(EnemyHero);
                        await Await.Delay(Basher.GetCastDelay(EnemyHero), token);
                    }

                    if (this.Mjollnir != null &&
                        base.Owner.IsAttacking() &&
                        this.Mjollnir.CanBeCasted &&
                        this.Config.ItemToggler.Value.IsEnabled(Mjollnir.ToString()))
                    {
                        this.Mjollnir.UseAbility(base.Owner);
                        await Await.Delay(Mjollnir.GetCastDelay(Owner), token);
                    }

                    if (!UnitExtensions.IsMagicImmune(EnemyHero)
                        && !EnemyHero.IsInvulnerable()
                        && !UnitExtensions.HasModifier(EnemyHero, "modifier_winter_wyvern_winters_curse"))
                    {
                        if (this.BloodThorn != null &&
                            this.BloodThorn.CanBeCasted &&
                            this.BloodThorn.CanHit(EnemyHero) &&
                            this.Config.ItemToggler.Value.IsEnabled(this.BloodThorn.ToString()))
                        {
                            this.BloodThorn.UseAbility(EnemyHero);
                            await Await.Delay(BloodThorn.GetCastDelay(EnemyHero), token);
                        }

                        if (this.DiffBlade != null &&
                            this.DiffBlade.CanBeCasted &&
                            this.DiffBlade.CanHit(EnemyHero) &&
                            this.Config.ItemToggler.Value.IsEnabled("item_diffusal_blade_2"))
                        {
                            this.DiffBlade.UseAbility(EnemyHero);
                            await Await.Delay(DiffBlade.GetCastDelay(EnemyHero), token);
                        }

                        if (this.Orchid != null &&
                            this.Orchid.CanBeCasted &&
                            this.Orchid.CanHit(EnemyHero) &&
                            this.Config.ItemToggler.Value.IsEnabled(Orchid.ToString()))
                        {
                            this.Orchid.UseAbility(EnemyHero);
                            await Await.Delay(Orchid.GetCastDelay(EnemyHero), token);
                        }

                        if (this.Heaven != null &&
                            base.Owner.IsAttacking() &&
                            this.Heaven.CanBeCasted &&
                            this.Heaven.CanHit(EnemyHero) &&
                            this.Config.ItemToggler.Value.IsEnabled(Heaven.ToString()))
                        {
                            this.Heaven.UseAbility(EnemyHero);
                            await Await.Delay(Heaven.GetCastDelay(EnemyHero), token);
                        }
                    }

                    if (EnemyHero != null && (EnemyHero.IsInvulnerable() || UnitExtensions.IsAttackImmune(EnemyHero)))
                    {
                        Orbwalker.Move(Game.MousePosition);
                    }
                    else if (EnemyHero != null)
                    {
                        Orbwalker.OrbwalkTo(EnemyHero);
                    }
                }
                else
                {
                    Orbwalker.Move(Game.MousePosition);
                }
            }
            catch (TaskCanceledException)
            {
                // canceled
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private int GetAbilityDelay(Unit unit, Ability ability)
        {
            return (int)(((ability.FindCastPoint() + this.Owner.GetTurnTime(unit)) * 1000.0) + Game.Ping) + 50;
        }

        private int GetAbilityDelay(Vector3 pos, Ability ability)
        {
            return (int)(((ability.FindCastPoint() + this.Owner.GetTurnTime(pos)) * 1000.0) + Game.Ping) + 50;
        }

        protected override void OnActivate()
        {
            this.Rage = UnitExtensions.GetAbilityById(this.Owner, AbilityId.life_stealer_rage);
            this.OpenWounds = UnitExtensions.GetAbilityById(this.Owner, AbilityId.life_stealer_open_wounds);

            this.Context.Inventory.Attach(this);

            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            this.Context.Inventory.Detach(this);
        }
    }
}