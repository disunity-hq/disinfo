using BindingAttributes;


namespace Disunity.Disinfo.Options {

    [Options("Discord")]
    public class DispatchServiceOptions {

        public string Prefix { get; set; } = "!";

    }

}