using RequireJsNet.Compressor;
using System.IO;

namespace RequireJSNet.Compressor.Tests.TestData
{
    public static class FileSpecs
    {
        public static FileSpec otherscripts_e(string projectPath, string shimDeps = "")
        {
            return new FileSpec(projectPath + @"\otherscripts\e.js", null)
            {
                FileContent = "define('externalfile', [" + shimDeps + "], function () {\r\nconsole.log('file-e.js');\r\n});"
            };
        }

        public static FileSpec Scripts_a(string projectPath)
        {
            return new FileSpec(projectPath + @"\Scripts\a.js", null)
            {
                FileContent = "define('a', [\"require\", \"exports\", \"b\"], function (require, exports, b) {\r\n    console.log('file-a.js');\r\n});"
            };
        }

        public static FileSpec Scripts_b(string projectPath)
        {
            return new FileSpec(projectPath + @"\Scripts\b.js", null)
            {
                FileContent = "define('b', [\"excludedfile\", \"c\", \"d\"], function ($, c, d) {\r\n    console.log('file-b.js');\r\n});"
            };
        }

        public static FileSpec Scripts_c(string projectPath)
        {
            return new FileSpec(projectPath + @"\Scripts\c.js", null)
            {
                FileContent = "define('c', [],function () {\r\n    console.log('file-c.js');\r\n});"
            };
        }

        public static FileSpec Scripts_d(string projectPath)
        {
            return new FileSpec(projectPath + @"\Scripts\d.js", null)
            {
                FileContent = "define('d', [],function () {\r\n    console.log('file-d.js');\r\n});"
            };
        }

        public static FileSpec Scripts_exportdefault(string projectPath)
        {
            return new FileSpec(projectPath + @"\Scripts\exportdefault.js", null)
            {
                FileContent =
@"define('exportdefault', [""require"", ""exports""], function (require, exports) {
    ""use strict"";
    function default_1(a, b) {
        return a + b;
    }
    Object.defineProperty(exports, ""__esModule"", { value: true });
    exports.default = default_1;
});"
            };
        }

        public static FileSpec Scripts_shimmed(string projectPath)
        {
            return new FileSpec(projectPath + @"\Scripts\shimmed.js", null)
            {
                FileContent = "define('shimmed', [],function () {\r\n    console.log('file-shimmed.js');\r\n});"
            };
        }

        public static FileSpec Scripts_BundleIncludedDirectory_a(string projectPath)
        {
            return new FileSpec(projectPath + @"\Scripts\BundleIncludedDirectory\a.js", null)
            {
                FileContent = "define('bundleincludeddirectory/a', [],function () {\r\n    console.log('a.js');\r\n});"
            };
        }

        public static FileSpec Scripts_BundleIncludedDirectory_b(string projectPath)
        {
            return new FileSpec(projectPath + @"\Scripts\BundleIncludedDirectory\b.js", null)
            {
                FileContent = "define('bundleincludeddirectory/b', [],function () {\r\n    console.log('b.js');\r\n});\r\n"
            };
        }

        public static FileSpec Scripts_MakeCorrectRelativePathWhenDependedFileRequiresUpwardsInTree_g(string projectPath)
        {
            return new FileSpec(projectPath + @"\Scripts\MakeCorrectRelativePathWhenDependedFileRequiresUpwardsInTree\g.js", null)
            {
                FileContent = "define('makecorrectrelativepathwhendependedfilerequiresupwardsintree/g', ['externalfile'],function () {\r\n    console.log('file-g.js');\r\n});"
            };
        }

        public static FileSpec Scripts_MakeCorrectRelativePathWhenDependedFileRequiresUpwardsInTree_f(string projectPath)
        {
            return new FileSpec(projectPath + @"\Scripts\MakeCorrectRelativePathWhenDependedFileRequiresUpwardsInTree\f.js", null)
            {
                FileContent = "define('makecorrectrelativepathwhendependedfilerequiresupwardsintree/f', ['./g'],function () {\r\n    console.log('starting-f.js');\r\n});"
            };
        }

        public static FileSpec Scripts_SupportEC6_h(string projectPath)
        {
            return new RequireJsNet.Compressor.FileSpec(projectPath + @"\Scripts\SupportEC6\h.js", null)
            {
                FileContent = "define('supportec6/h', ['require', 'exports'],function (require, exports) {\r\n    function default_1() { console.log('my fn'); };\r\n    Object.defineProperty(exports, \"__esModule\", { value: true });\r\n    exports.default = default_1;\r\n});"
            };
        }

    }
}
