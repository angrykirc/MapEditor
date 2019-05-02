using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MapEditor.render
{
    public class MapImageObjectFilter
    {
        private List<string> hideObjects;
        public bool Armor { set { AddRemove(value, armor); } }
        public bool Weapons { set { AddRemove(value, weapons); } }
        public bool Potions { set { AddRemove(value, potions); } }
        public bool Food { set { AddRemove(value, food); } }
        public bool Crystals { set { AddRemove(value, crystals); } }
        public bool Gold { set { AddRemove(value, gold); } }
        public bool Chests { set { AddRemove(value, chests); } }
        public bool Easy { set { AddRemove(value, easy); } }
        public bool Hard { set { AddRemove(value, hard); } }
        public bool Boss { set { AddRemove(value, boss); } }
        public bool Ambient { set { AddRemove(value, ambient); } }
        public bool Broken { set { AddRemove(value, broken); } }
        private string[] armor = new string[] { "STREETPANTS", "STREETSHIRT", "STREETSNEAKERS", "MEDIEVALCLOAK", "MEDIEVALPANTS", "MEDIEVALSHIRT", "WIZARDROBE", "WIZARDHELM", "CONJURERHELM", "LEATHERHELM", "LEATHERARMBANDS", "LEATHERBOOTS", "LEATHERARMOREDBOOTS", "LEATHERLEGGINGS", "LEATHERARMOR", "CHAINCOIF", "CHAINLEGGINGS", "CHAINMAIL", "CHAINTUNIC", "STEELHELM", "ORNATEHELM", "BREASTPLATE", "PLATELEGGINGS", "PLATEARMS", "PLATEBOOTS" };
        private string[] weapons = new string[] { "BATTLEAXE", "BOW", "CROSSBOW", "DAGGER", "DEATHRAYWAND", "FANCHAKRAM", "FIRESTORMWAND", "FORCEWAND", "GREATSWORD", "INFINITEPAINWAND", "LESSERFIREBALLWAND", "LONGSWORD", "MORNINGSTAR", "OBLIVIONHALBERD", "OBLIVIONHEART", "OBLIVIONORB", "OBLIVIONWIERDLING", "OGREAXE", "QUIVER", "ROUNDCHAKRAM", "STAFFWOODEN", "STEELSHIELD", "SULPHOROUSFLAREWAND", "SULPHOROUSSHOWERWAND", "SWORD", "WARHAMMER", "WEBWAND", "WOODENSHIELD" };
        private string[] potions = new string[] { "REDPOTION", "BLUEPOTION", "CUREPOISONPOTION", "FIREPROTECTPOTION", "HASTEPOTION", "INFRAVISIONPOTION", "INVISIBILITYPOTION", "INVULNERABILITYPOTION", "POISONPROTECTPOTION", "SHIELDPOTION", "SHOCKPROTECTPOTION", "VAMPIRISMPOTION" };
        private string[] food = new string[] { "BREAD", "CIDER", "CORN", "MEAT", "MUSHROOM", "REDAPPLE", "ROTTENMEAT", "SOUP" };
        private string[] crystals = new string[] { "MANACRYSTALCLUSTER", "MANACRYSTALLARGE", "MANACRYSTALSMALL" };
        private string[] gold = new string[] { "GOLD", "QUESTGOLDCHEST", "QUESTGOLDPILE" };
        private string[] chests = new string[] { "CHEST1", "CHEST2", "CHEST3", "CHEST4", "CHESTLOTD1", "CHESTLOTD2", "CHESTLOTD3", "CHESTLOTD4", "CHESTNE", "CHESTNW", "CHESTOGRE1", "CHESTOGRE2", "CHESTOGRE3", "CHESTOGRE4", "CHESTSE", "CHESTSW", "CHESTURCHIN1", "CHESTURCHIN2", "CHESTURCHIN3", "CHESTURCHIN4", "DUNMIRCHEST1", "DUNMIRCHEST2", "DUNMIRCHEST3", "DUNMIRCHEST4", "OPENCHEST1", "OPENCHEST2", "OPENCHEST3", "OPENCHEST4", "OPENCHESTNE", "OPENCHESTNW", "OPENCHESTSE", "OPENCHESTSW" };
        private string[] easy = new string[] { "ALBINOSPIDER", "ARCHER", "BAT", "GIANTLEECH", "IMP", "MELEEDEMON", "SMALLALBINOSPIDER", "SMALLSPIDER", "SWORDSMAN", "URCHIN", "WASP", "WOLF", "ZOMBIE", "WHITEWOLF" };
        private string[] hard = new string[] { "BEAR", "BLACKBEAR", "BLACKWOLF", "CARNIVOROUSPLANT", "EVILCHERUB", "FLYINGOLEM", "GHOST", "GRUNTAXE", "OGREBRUTE", "SCORPION", "SHADE", "SKELETON", "SPIDER", "SPITTINGSPIDER", "TROLL", "URCHINSHAMAN", "VILEZOMBIE", "WILLOWISP" };
        private string[] boss = new string[] { "BEHOLDER", "DEMON", "EMBERDEMON", "HORRENDOUS", "LICH", "LICHLORD", "MECHANICALGOLEM", "MIMIC", "OGREWARLORD", "SKELETONLORD", "STONEGOLEM", "WIZARD", "WIZARDGREEN", "WIZARDWHITE" };
        private string[] ambient = new string[] { "AIRSHIPCAPTAIN", "FISHBIG", "FISHSMALL", "GREENFROG", "RAT", "MAIDEN", "TALKINGSKULL", "SHOPKEEPER", "SHOPKEEPERCONJURERREALM", "SHOPKEEPERLANDOFTHEDEAD", "SHOPKEEPERMAGICSHOP", "SHOPKEEPERPURPLE", "SHOPKEEPERWARRIORSREALM", "SHOPKEEPERWIZARDREALM", "SHOPKEEPERYELLOW" };
        private string[] broken = new string[] { "BOMBER", "BOMBERBLUE", "BOMBERGREEN", "BOMBERYELLOW", "BEAR2", "BLACKWIDOW", "GOON", "FIRESPRITE", "HECUBAH", "HECUBAHWITHORB", "NECROMANCER", "NPC", "NPCWIZARD", "STRONGWIZARDWHITE", "WEIRDLINGBEAST", "WIZARDRED" };

        public MapImageObjectFilter()
        {
            hideObjects = new List<string>();
            hideObjects.Add("PLAYERSTART");
        }
        public bool HideObject(string obj)
        {
            if (hideObjects.Contains(obj.ToUpper()))
                return true;

            return false;
        }
        private void AddRemove(bool show, string[] objs)
        {
            if (!show)
                hideObjects.AddRange(objs);
            else
                hideObjects.RemoveAll(x => objs.Contains(x));
        }
    }
}
