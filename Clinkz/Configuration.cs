namespace Clinkz
{
    using Ensage;
    using Ensage.Common.Menu;
    using Ensage.SDK.Menu;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class Configuration : IDisposable
    {
        public readonly MenuFactory MFactory;

        public Configuration(string ownerName)
        {

            var itemTab = new List<AbilityId>
            {
                AbilityId.item_medallion_of_courage,
                AbilityId.item_solar_crest,
                AbilityId.item_sheepstick,
                AbilityId.item_diffusal_blade,
                AbilityId.item_nullifier,
                AbilityId.item_orchid,
                AbilityId.item_bloodthorn,
                AbilityId.item_mjollnir
            };

            var spellTab = new List<AbilityId>
            {
                AbilityId.clinkz_death_pact,
                AbilityId.clinkz_searing_arrows,
                AbilityId.clinkz_strafe
            };




            MFactory = MenuFactory.CreateWithTexture("Clinkz", ownerName);



            DrawTargetParticle = MFactory.Item("Draw target particle", true);

            
            ComboKey = MFactory.Item("Combo Key", new KeyBind(32));

            var itemMenu = MFactory.Menu("Item Manager");
            ItemManager = itemMenu.Item("Item Toggler:  ", new AbilityToggler(itemTab.ToDictionary(x => x.ToString(), x => true)));

            var abilityMenu = MFactory.Menu("Ability Manager");
            AbilityManager = abilityMenu.Item("Ability Toggler:  ", new AbilityToggler(spellTab.ToDictionary(x => x.ToString(), x => true)));

        }



        public MenuItem<KeyBind> ComboKey { get; }

        public MenuItem<bool> DrawTargetParticle { get; }

        public MenuItem<AbilityToggler> ItemManager { get; }

        public MenuItem<AbilityToggler> AbilityManager { get; }

        public void Dispose()
        {
            MFactory.Dispose(); 
        }
    }
}
