using System.ComponentModel.DataAnnotations;

using BindingAttributes;


namespace Disunity.Disinfo.Options {

    [Options("Db")]
    public class DbServiceOptions {

        [Required]
        public string Path { get; set; }
    }

}