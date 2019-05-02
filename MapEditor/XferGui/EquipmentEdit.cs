/*
 * MapEditor
 * Пользователь: AngryKirC
 * Дата: 30.06.2015
 */
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MapEditor.render;
using NoxShared;
using NoxShared.ObjDataXfer;

namespace MapEditor.XferGui
{
	public partial class EquipmentEdit : XferEditor
	{
		#region (Enchantment Ids)
		public static string[] ENCHANTMENTS = new string[] {
										"WeaponPower1",
										"WeaponPower2",
										"WeaponPower3",
										"WeaponPower4",
										"WeaponPower5",
										"WeaponPower6",
										"ArmorQuality1",
										"ArmorQuality2",
										"ArmorQuality3",
										"ArmorQuality4",
										"ArmorQuality5",
										"ArmorQuality6",
										"Material1",
										"Material2",
										"Material3",
										"Material4",
										"Material5",
										"Material6",
										"Material7",
										"MaterialTeamRed",
										"MaterialTeamGreen",
										"MaterialTeamBlue",
										"MaterialTeamYellow",
										"MaterialTeamCyan",
										"MaterialTeamViolet",
										"MaterialTeamBlack",
										"MaterialTeamWhite",
										"MaterialTeamOrange",
										"Stun1",
										"Stun2",
										"Stun3",
										"Stun4",
										"Fire1",
										"Fire2",
										"Fire3",
										"Fire4",
										"FireRing1",
										"FireRing2",
										"FireRing3",
										"FireRing4",
										"BlueFireRing1",
										"BlueFireRing2",
										"BlueFireRing3",
										"BlueFireRing4",
										"Impact1",
										"Impact2",
										"Impact3",
										"Impact4",
										"Confuse1",
										"Confuse2",
										"Confuse3",
										"Confuse4",
										"Lightning1",
										"Lightning2",
										"Lightning3",
										"Lightning4",
										"ManaSteal1",
										"ManaSteal2",
										"ManaSteal3",
										"ManaSteal4",
										"Vampirism1",
										"Vampirism2",
										"Vampirism3",
										"Vampirism4",
										"Venom1",
										"Venom2",
										"Venom3",
										"Venom4",
										"Brilliance1",
										"FireProtect1",
										"FireProtect2",
										"FireProtect3",
										"FireProtect4",
										"LightningProtect1",
										"LightningProtect2",
										"LightningProtect3",
										"LightningProtect4",
										"Regeneration1",
										"Regeneration2",
										"Regeneration3",
										"Regeneration4",
										"PoisonProtect1",
										"PoisonProtect2",
										"PoisonProtect3",
										"PoisonProtect4",
										"Speed1",
										"Speed2",
										"Speed3",
										"Speed4",
										"Readiness1",
										"Readiness2",
										"Readiness3",
										"Readiness4",
										"ProjectileSpeed1",
										"ProjectileSpeed2",
										"ProjectileSpeed3",
										"ProjectileSpeed4",
										"Replenishment1",
										"ContinualReplenishment1",
										"UserColor1",
										"UserColor2",
										"UserColor3",
										"UserColor4",
										"UserColor5",
										"UserColor6",
										"UserColor7",
										"UserColor8",
										"UserColor9",
										"UserColor10",
										"UserColor11",
										"UserColor12",
										"UserColor13",
										"UserColor14",
										"UserColor15",
										"UserColor16",
										"UserColor17",
										"UserColor18",
										"UserColor19",
										"UserColor20",
										"UserColor21",
										"UserColor22",
										"UserColor23",
										"UserColor24",
										"UserColor25",
										"UserColor26",
										"UserColor27",
										"UserColor28",
										"UserColor29",
										"UserColor30",
										"UserColor31",
										"UserColor32",
										"UserColor33",
										"UserMaterialColor1",
										"UserMaterialColor2",
										"UserMaterialColor3",
										"UserMaterialColor4",
										"UserMaterialColor5",
										"UserMaterialColor6",
										"UserMaterialColor7",
										"UserMaterialColor8",
										"UserMaterialColor9",
										"UserMaterialColor10",
										"UserMaterialColor11",
										"UserMaterialColor12",
										"UserMaterialColor13",
										"UserMaterialColor14",
										"UserMaterialColor15",
										"UserMaterialColor16",
										"UserMaterialColor17",
										"UserMaterialColor18",
										"UserMaterialColor19",
										"UserMaterialColor20",
										"UserMaterialColor21",
										"UserMaterialColor22",
										"UserMaterialColor23",
										"UserMaterialColor24",
										"UserMaterialColor25",
										"UserMaterialColor26",
										"UserMaterialColor27",
										"UserMaterialColor28",
										"UserMaterialColor29",
										"UserMaterialColor30",
										"UserMaterialColor31",
										"UserMaterialColor32"
		};
        #endregion

        private bool pausePreview = true;
        private bool randClicked = false;

        public EquipmentEdit()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			// TODO: Implement loading enchantments on runtime from modifier.bin
			enchantment1.Items.AddRange(ENCHANTMENTS);
			enchantment2.Items.AddRange(ENCHANTMENTS);
			enchantment3.Items.AddRange(ENCHANTMENTS);
			enchantment4.Items.AddRange(ENCHANTMENTS);
		}
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        public override void SetDefaultData(Map.Object obj)
		{
			base.SetDefaultData(obj);
			
			// Default values for armor and weapons
			ThingDb.Thing tt = ThingDb.Things[obj.Name];
            if (tt.Xfer == "WeaponXfer")
            {
                obj.GetExtraData<WeaponXfer>().Durability = GetDurability(tt);
                obj.GetExtraData<WeaponXfer>().DefaultsFor(tt);
            }
            else if (tt.Xfer == "ArmorXfer")
                obj.GetExtraData<ArmorXfer>().Durability = GetDurability(tt);
				
		}
        private short GetDurability(ThingDb.Thing thing)
        {
            // Maps will crash without proper durability, pulled from Westwood maps
            short result = (short) thing.Health;

            // Armor
            if (thing.Name == "OrnateHelm") result = 850;
            else if (thing.Name == "SteelHelm") result = 675;
            else if (thing.Name == "Breastplate") result = 800;
            else if (thing.Name == "PlateLeggings") result = 700;
            else if (thing.Name == "PlateArms") result = 600;
            else if (thing.Name == "PlateBoots") result = 700;
            else if (thing.Name == "MedievalCloak") result = 200;
            else if (thing.Name == "ChainCoif") result = 325;
            else if (thing.Name == "ChainLeggings") result = 325;
            else if (thing.Name == "ChainTunic") result = 400;
            else if (thing.Name == "ConjurerHelm") result = 500;
            else if (thing.Name == "LeatherHelm") result = 200;
            else if (thing.Name == "LeatherArmbands") result = 200;
            else if (thing.Name == "LeatherArmoredBoots") result = 300;
            else if (thing.Name == "LeatherLeggings") result = 200;
            else if (thing.Name == "LeatherArmor") result = 300;
            else if (thing.Name == "LeatherBoots") result = 200;
            else if (thing.Name == "WizardRobe") result = 325;
            else if (thing.Name == "WizardHelm") result = 350;

            // Weapons
            else if (thing.Name == "SteelShield") result = 300;
            else if (thing.Name == "WoodenShield") result = 200;
            else if (thing.Name == "OgreAxe") result = 50;
            else if (thing.Name == "Sword") result = 160; // 200
            else if (thing.Name == "Longsword") result = 180;
            else if (thing.Name == "MorningStar") result = 200; // 250
            else if (thing.Name == "BattleAxe") result = 220;
            else if (thing.Name == "RoundChakram") result = 300;
            else if (thing.Name == "WarHammer") result = 350;
            else if (thing.Name == "GreatSword") result = 400;
            else if (thing.Name == "DeathRayWand") result = 70;
            else if (thing.Name == "LesserFireballWand") result = 100;
            else if (thing.Name == "FireStormWand") result = 100;
            else if (thing.Name == "ForceWand") result = 110;
            else if (thing.Name == "StaffWooden") result = 120;
            else if (thing.Name == "InfinitePainWand") result = 80;
            else if (thing.Name == "CrossBow") result = 500;
            else if (thing.Name == "Bow") result = 500;

            return result;
        }
		
		public override void SetObject(Map.Object obj)
		{
			this.obj = obj;
			ThingDb.Thing tt = ThingDb.Things[obj.Name];
			WeaponXfer weapon; ArmorXfer armor; AmmoXfer ammo; TeamXfer team;
			
			switch (tt.Xfer)
			{
				case "WeaponXfer":
					weapon = obj.GetExtraData<WeaponXfer>();
					enchantment1.Text = weapon.Enchantments[0];
					enchantment2.Text = weapon.Enchantments[1];
					enchantment3.Text = weapon.Enchantments[2];
					enchantment4.Text = weapon.Enchantments[3];
					durability.Enabled = true; durability.Value = weapon.Durability;
					if (tt.HasClassFlag(ThingDb.Thing.ClassFlags.WAND) && !tt.Subclass[(int) ThingDb.Thing.SubclassBitIndex.STAFF])
					{
						ammoMin.Enabled = true; ammoMax.Enabled = true;
						ammoMin.Value = weapon.WandChargesCurrent;
						ammoMax.Value = weapon.WandChargesLimit;
					}
					break;
				case "ArmorXfer":
					armor = obj.GetExtraData<ArmorXfer>();
					enchantment1.Text = armor.Enchantments[0];
					enchantment2.Text = armor.Enchantments[1];
					enchantment3.Text = armor.Enchantments[2];
					enchantment4.Text = armor.Enchantments[3];
					durability.Enabled = true; durability.Value = armor.Durability;
					break;
				case "AmmoXfer":
					ammo = obj.GetExtraData<AmmoXfer>();
					enchantment1.Text = ammo.Enchantments[0];
					enchantment2.Text = ammo.Enchantments[1];
					enchantment3.Text = ammo.Enchantments[2];
					enchantment4.Text = ammo.Enchantments[3];
					ammoMin.Enabled = true; ammoMax.Enabled = true;
					ammoMin.Value = ammo.AmmoCurrent; ammoMax.Value = ammo.AmmoLimit;
					break;
				case "TeamXfer":
					team = obj.GetExtraData<TeamXfer>();
					enchantment1.Text = team.Enchantments[0];
					enchantment2.Text = team.Enchantments[1];
					enchantment3.Text = team.Enchantments[2];
					enchantment4.Text = team.Enchantments[3];
					break;
			}
            pausePreview = false;
		}
		public override Map.Object GetObject()
		{
			ThingDb.Thing tt = ThingDb.Things[obj.Name];
			WeaponXfer weapon; ArmorXfer armor; AmmoXfer ammo; TeamXfer team;
			
			switch (tt.Xfer)
			{
				case "WeaponXfer":
					weapon = obj.GetExtraData<WeaponXfer>();
					weapon.Enchantments[0] = enchantment1.Text;
					weapon.Enchantments[1] = enchantment2.Text;
					weapon.Enchantments[2] = enchantment3.Text;
					weapon.Enchantments[3] = enchantment4.Text;
					weapon.Durability = (short) durability.Value;
					if (ammoMin.Enabled && ammoMax.Enabled)
					{
						weapon.WandChargesCurrent = (byte) ammoMin.Value;
						weapon.WandChargesLimit = (byte) ammoMax.Value;
					}
					break;
				case "ArmorXfer":
					armor = obj.GetExtraData<ArmorXfer>();
					armor.Enchantments[0] = enchantment1.Text;
					armor.Enchantments[1] = enchantment2.Text;
					armor.Enchantments[2] = enchantment3.Text;
					armor.Enchantments[3] = enchantment4.Text;
					armor.Durability = (short) durability.Value;
					break;
				case "AmmoXfer":
					ammo = obj.GetExtraData<AmmoXfer>();
					ammo.Enchantments[0] = enchantment1.Text;
					ammo.Enchantments[1] = enchantment2.Text;
					ammo.Enchantments[2] = enchantment3.Text;
					ammo.Enchantments[3] = enchantment4.Text;
					ammo.AmmoCurrent = (byte) ammoMin.Value;
					ammo.AmmoLimit = (byte) ammoMax.Value;
					break;
				case "TeamXfer":
					team = obj.GetExtraData<TeamXfer>();
					team.Enchantments[0] = enchantment1.Text;
					team.Enchantments[1] = enchantment2.Text;
					team.Enchantments[2] = enchantment3.Text;
					team.Enchantments[3] = enchantment4.Text;
					break;
			}
			return obj;
		}
        public Map.Object GetClonedObject()
        {
            Map.Object newObj = new Map.Object(obj.Name, new PointF(5, 5));
            ThingDb.Thing tt = ThingDb.Things[newObj.Name];
            WeaponXfer weapon; ArmorXfer armor; AmmoXfer ammo; TeamXfer team;

            var enchants = new string[] { enchantment1.Text, enchantment2.Text, enchantment3.Text, enchantment4.Text };

            switch (tt.Xfer)
            {
                case "WeaponXfer":
                    weapon = newObj.GetExtraData<WeaponXfer>();
                    weapon.Enchantments = enchants;
                    weapon.Durability = (short)durability.Value;
                    if (ammoMin.Enabled && ammoMax.Enabled)
                    {
                        weapon.WandChargesCurrent = (byte)ammoMin.Value;
                        weapon.WandChargesLimit = (byte)ammoMax.Value;
                    }
                    break;
                case "ArmorXfer":
                    armor = newObj.GetExtraData<ArmorXfer>();
                    armor.Enchantments = enchants;
                    armor.Durability = (short)durability.Value;
                    break;
                case "AmmoXfer":
                    ammo = newObj.GetExtraData<AmmoXfer>();
                    ammo.Enchantments = enchants;
                    ammo.AmmoCurrent = (byte)ammoMin.Value;
                    ammo.AmmoLimit = (byte)ammoMax.Value;
                    break;
                case "TeamXfer":
                    team = newObj.GetExtraData<TeamXfer>();
                    team.Enchantments = enchants;
                    break;
            }
            return newObj;
        }

        #region Preview Events
        private void picRender_Click(object sender, EventArgs e)
        {
            if (picRender.BackColor == Color.Black)
                picRender.BackColor = Color.White;
            else
                picRender.BackColor = Color.Black;
        }
        private void picRenderOrig_Click(object sender, EventArgs e)
        {
            if (picRenderOrig.BackColor == Color.Black)
                picRenderOrig.BackColor = Color.White;
            else
                picRenderOrig.BackColor = Color.Black;
        }
        private void EquipmentEdit_Load(object sender, EventArgs e)
        {
            SetPreviews();
        }
        private void EquipmentEdit_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (randClicked)
            {
                e.Cancel = true;
                randClicked = false;
            }
        }
        private void enchantment1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetPreviews();
        }
        private void enchantment2_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetPreviews();
        }
        private void enchantment3_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetPreviews();
        }
        private void enchantment4_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetPreviews();
        }
        private void buttonRandomize_Click(object sender, EventArgs e)
        {
            pausePreview = true;
            randClicked = true;
            var rand = new Random();
            enchantment1.SelectedIndex = rand.Next(0, enchantment1.Items.Count);
            enchantment2.SelectedIndex = rand.Next(0, enchantment2.Items.Count);
            enchantment3.SelectedIndex = rand.Next(0, enchantment3.Items.Count);
            enchantment4.SelectedIndex = rand.Next(0, enchantment4.Items.Count);
            pausePreview = false;
            SetPreviews();
        }
        
        private void SetPreviews()
        {
            var img = GetPreview();
            if (img != null)
            {
                picRenderOrig.BackgroundImage = img;
                picRender.BackgroundImage = ImageHelper.ResizeImage(img, new Size(90, 90), true);
            }
        }
        private Bitmap GetPreview()
        {
            if (obj == null)
                return null;
            if (pausePreview)
                return null;

            var newObj = GetClonedObject();
            var tt = ThingDb.Things[newObj.Name];

            var r = new ObjectRenderer(MainWindow.Instance.mapView.MapRenderer);
            Bitmap img = r.GetObjectImageSpecial(newObj, tt, false);

            return img;
        }
        #endregion
    }
}
