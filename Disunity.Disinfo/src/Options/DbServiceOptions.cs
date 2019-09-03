using System.ComponentModel.DataAnnotations;

using BindingAttributes;

using Discord;
using Discord.WebSocket;


namespace Disunity.Disinfo.Options {

    [Options("Db")]
    public class DbServiceOptions {

        [Required]
        public string Path { get; set; }
    }

}