using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGameAssets.PackData
{
    public class GiftTastePackData : BasePackData
    {
        public string ObjectId { get; set; }
        public string Npc { get; set; }

        public int Amount { get; set; }
        [DefaultValue( null )]
        public string NormalTextTranslationKey { get; set; }
        [DefaultValue( null )]
        public string BirthdayTextTranslationKey { get; set; }
        [DefaultValue( null )]
        public int? EmoteId { get; set; }
    }
}
