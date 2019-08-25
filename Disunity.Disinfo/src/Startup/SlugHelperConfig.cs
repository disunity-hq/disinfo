using BindingAttributes;

using Slugify;


namespace Disunity.Disinfo.Startup {

    [AsSingleton]
    public class SlugHelperConfig : SlugHelper.Config {

        public SlugHelperConfig() {
            CollapseDashes = true;
            CollapseWhiteSpace = true;
            ForceLowerCase = true;
        }

    }

}