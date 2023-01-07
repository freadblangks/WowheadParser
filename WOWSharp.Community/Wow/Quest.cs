
using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace WOWSharp.Community.Wow
{
    public class Quest : ApiResponse
    {
        [JsonProperty("_links")]
        public Links Links { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("area")]
        public Area Area { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("requirements")]
        public QuestRequirements Requirements { get; set; }

        [JsonProperty("rewards")]
        public Rewards Rewards { get; set; }
    }

    public class Area
    {
        [JsonProperty("key")]
        public Self Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }
    }

    public class Self
    {
        [JsonProperty("href")]
        public Uri Href { get; set; }
    }

    public class Links
    {
        [JsonProperty("self")]
        public Self Self { get; set; }
    }

    public class QuestRequirements
    {
        [JsonProperty("min_character_level")]
        public long MinCharacterLevel { get; set; }

        [JsonProperty("max_character_level")]
        public long MaxCharacterLevel { get; set; }

        [JsonProperty("faction")]
        public FactionObj Faction { get; set; }

        [JsonProperty("reputations")]
        public RequirementsReputation[] Reputations { get; set; }
    }

    public class FactionObj
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("key")]
        public Self key { get; set; }
    }

    public class RequirementsReputation
    {
        [JsonProperty("faction")]
        public Area Faction { get; set; }

        [JsonProperty("min_reputation", NullValueHandling = NullValueHandling.Ignore)]
        public long? MinReputation { get; set; }

        [JsonProperty("max_reputation", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxReputation { get; set; }
    }

    public class Rewards
    {
        [JsonProperty("experience")]
        public long Experience { get; set; }

        [JsonProperty("reputations")]
        public RewardsReputation[] Reputations { get; set; }

        [JsonProperty("money")]
        public Money Money { get; set; }

        [JsonProperty("items")]
        public Items Items { get; set; }
    }

    public class Items
    {
        [JsonProperty("items")]
        public QItem[] ItemsItems { get; set; }

        [JsonProperty("choice_of")]
        public ChoiceOf[] ChoiceOf { get; set; }
    }

    public class ChoiceOf
    {
        [JsonProperty("item")]
        public Area Item { get; set; }

        [JsonProperty("requirements")]
        public ChoiceOfRequirements Requirements { get; set; }
    }

    public class ChoiceOfRequirements
    {
        [JsonProperty("playable_specializations")]
        public Area[] PlayableSpecializations { get; set; }
    }

    public class QItem
    {
        [JsonProperty("item")]
        public Area ItemItem { get; set; }
    }

    public class Money
    {
        [JsonProperty("value")]
        public long Value { get; set; }

        [JsonProperty("units")]
        public Units Units { get; set; }
    }

    public class Units
    {
        [JsonProperty("gold")]
        public long Gold { get; set; }

        [JsonProperty("silver")]
        public long Silver { get; set; }

        [JsonProperty("copper")]
        public long Copper { get; set; }
    }

    public class RewardsReputation
    {
        [JsonProperty("reward")]
        public Area Reward { get; set; }

        [JsonProperty("value")]
        public long Value { get; set; }
    }



    /// <summary>
    ///   Represents quest information
    /// </summary>
    [DataContract]
    public class Ques1t : ApiResponse
    {
        /// <summary>
        ///   Gets or sets the achievement id
        /// </summary>
        [DataMember(Name = "id", IsRequired = true)]
        public int Id
        {
            get;
            internal set;
        }

        /// <summary>
        ///   Gets or sets the achievement title
        /// </summary>
        [DataMember(Name = "title", IsRequired = true)]
        public string Title
        {
            get;
            internal set;
        }

        /// <summary>
        ///   Gets or sets the required level
        /// </summary>
        [DataMember(Name = "reqLevel", IsRequired = false)]
        public int RequiredLevel
        {
            get;
            internal set;
        }

        /// <summary>
        ///   Gets or sets the number of suggested party members
        /// </summary>
        [DataMember(Name = "suggestedPartyMembers", IsRequired = false)]
        public int SuggestedPartyMembers
        {
            get;
            internal set;
        }

        /// <summary>
        ///   Gets or sets the quest's category
        /// </summary>
        [DataMember(Name = "category", IsRequired = false)]
        public string Category
        {
            get;
            internal set;
        }

        /// <summary>
        ///   Gets or sets the quest's level (I am guessing that's the recommended level?)
        /// </summary>
        [DataMember(Name = "level", IsRequired = false)]
        public int Level
        {
            get;
            internal set;
        }

        /// <summary>
        ///   Gets string representation (for debugging purposes)
        /// </summary>
        /// <returns> Gets string representation (for debugging purposes) </returns>
        public override string ToString()
        {
            return Title;
        }
    }
}