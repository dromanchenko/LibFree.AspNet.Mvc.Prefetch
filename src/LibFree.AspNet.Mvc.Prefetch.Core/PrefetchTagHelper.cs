using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.Logging;
using System.IO;
using System.Text;

namespace LibFree.AspNet.Mvc.Prefetch.Core
{
	[TargetElement("prefetch")]
	public sealed class PrefetchTagHelper : TagHelper
	{
		[HtmlAttributeName("directory")]
		public string Directory { get; set; }

		private ILogger _logger;
		private IHostingEnvironment _hostingEnvironment;

		private static string _linksCache;
		private static readonly object _linkCacheLockObject = new object();

		public PrefetchTagHelper(ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment)
		{
			_logger = loggerFactory.CreateLogger<PrefetchTagHelper>();
			_hostingEnvironment = hostingEnvironment;
		}

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			_logger.LogVerbose("PrefetchTagHelper: running");

			output.SuppressOutput();

			string links;
			if (_linksCache != null)
			{
				links = _linksCache;
			}
			else
			{
				lock (_linkCacheLockObject)
				{
					if (_linksCache != null)
					{
						links = _linksCache;
					}
					else
					{
						_linksCache = BuildLinks();
						links = _linksCache;
					}
				}
			}

			output.Content.SetContent(links);
		}

		private string BuildLinks()
		{
			var linksStringBuilder = new StringBuilder();

			var normalizedDirectoryPath = Directory;
			if (normalizedDirectoryPath[0] == '/' || normalizedDirectoryPath[0] == '\\')
			{
				normalizedDirectoryPath = normalizedDirectoryPath.Remove(0, 1);
			}

			var physicalPath = Path.Combine(_hostingEnvironment.WebRootPath, normalizedDirectoryPath);
			var directoryInfo = new DirectoryInfo(physicalPath);
			BuildLinks(directoryInfo, linksStringBuilder);
			return linksStringBuilder.ToString();
		}

		private void BuildLinks(DirectoryInfo directoryInfo, StringBuilder linksStringBuilder)
		{
			foreach (var fileInfo in directoryInfo.EnumerateFiles())
			{
				var relativeName = fileInfo.FullName.Replace(_hostingEnvironment.WebRootPath, string.Empty)
					.Replace("\\", "/");
				linksStringBuilder.AppendFormat("<link rel=\"prefetch\" href=\"{0}\" />\r\n", relativeName);
			}

			foreach (var childDirectoryInfo in directoryInfo.EnumerateDirectories())
			{
				BuildLinks(childDirectoryInfo, linksStringBuilder);
			}
		}
	}
}