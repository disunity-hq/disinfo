using System.ComponentModel.DataAnnotations;

using BindingAttributes;


namespace Disunity.Disinfo.Options {

    [Options("Db")]
    public class EmbedDBContextOptions {

        [Required]
        public string Filename { get; set; }
        
    }

}