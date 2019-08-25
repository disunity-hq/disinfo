using Slugify;


namespace Disunity.Disinfo.Startup {

    public class SlugHelperConfig : SlugHelper.Config {

        public SlugHelperConfig() {
            CollapseDashes = true;
            CollapseWhiteSpace = true;
            ForceLowerCase = true;
        }

    }

}