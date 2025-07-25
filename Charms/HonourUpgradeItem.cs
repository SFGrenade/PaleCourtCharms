using System.Collections.Generic;
using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using Modding;
using UnityEngine;

namespace PaleCourtCharms
{
 public class HonourUpgradeItem : AbstractItem
{
   
    private const string HonourKey = "Kings_Honour";

    public HonourUpgradeItem()
    {
        name = HonourKey;
        UIDef = new MsgUIDef
        {
            name     = new BoxedString("King’s Honour"),
            shopDesc = new BoxedString("Ogrim left me this,i think you may meed it more.\n\nUpgrades Defender’s Crest into King’s Honour."),
            sprite = new ICShiny.EmbeddedSprite { key = HonourKey }

        };


    }

        public override void GiveImmediate(GiveInfo info)
        {

            PaleCourtCharms.Settings.upgradedCharm_10 = true;
            PlayerData.instance.SetBool("upgradedCharm_10", true);
            
        GameManager.instance.SaveGame();
    }

    public override bool Redundant()
    {
        
        return PlayerData.instance.GetBool("gotCharm_10") &&
               PaleCourtCharms.Settings.upgradedCharm_10;
    }
}

}
