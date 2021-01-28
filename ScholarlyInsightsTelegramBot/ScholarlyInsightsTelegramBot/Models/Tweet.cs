using System;
using System.Collections.Generic;
using System.Text;

namespace ScholarlyInsightsTelegramBot.Models
{
    class Tweet
    {
        public DateTime postDate { get; set; }

        //can be found within the time tag in the article
        public string tweetText { get; set; }
    }
}
