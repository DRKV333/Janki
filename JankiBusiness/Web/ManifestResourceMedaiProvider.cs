using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JankiBusiness.Web
{
    public class ManifestResourceMedaiProvider : IMediaProvider
    {
        private readonly Assembly assembly;
        private readonly IDictionary<string, string> resources;

        public ManifestResourceMedaiProvider(Assembly assembly, IDictionary<string, string> resources)
        {
            this.assembly = assembly;
            this.resources = resources;
        }

        public static readonly ManifestResourceMedaiProvider MathJax =
            new ManifestResourceMedaiProvider(typeof(ManifestResourceMedaiProvider).Assembly,
                new Dictionary<string, string>()
                {
                    ["tex-mml-chtml.js"] = "JankiBusiness.Web.Assets.mathjax.tex-mml-chtml.js"
                });

        public static readonly ManifestResourceMedaiProvider FieldEditor =
            new ManifestResourceMedaiProvider(typeof(ManifestResourceMedaiProvider).Assembly,
                new Dictionary<string, string>()
                {
                    ["fieldeditor.html"] = "JankiBusiness.Web.Assets.FieldEditor.FieldEditor.html",
                    ["squire/squire.js"] = "JankiBusiness.Web.Assets.FieldEditor.Squire.squire.js"
                });

        public Task<Stream> GetMediaStream(string name)
        {
            if (resources.TryGetValue(name.ToLower(), out string value))
                return Task.FromResult(assembly.GetManifestResourceStream(value));
            return Task.FromResult<Stream>(null);
        }
    }
}
