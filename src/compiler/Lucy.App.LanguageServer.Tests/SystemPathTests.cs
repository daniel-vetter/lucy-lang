using Lucy.App.LanguageServer.Infrastructure;
using Shouldly;
using System;
using System.Runtime.InteropServices;
using Xunit;

namespace Lucy.App.LanguageServer.Tests
{
    public class SystemPathTests
    {
        [Fact]
        public void Paths_should_be_parsed_correctly()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                new SystemPath("file:///c:/temp").ToString().ShouldBe("c:\\temp");
                new SystemPath("file:///c%3a/temp").ToString().ShouldBe("c:\\temp");
                new SystemPath("c:\\temp").ToString().ShouldBe("c:\\temp");
            }
            else
            {
                new SystemPath("file:///c:/temp").ToString().ShouldBe("/c:/temp");
                new SystemPath("file:///c%3a/temp").ToString().ShouldBe("/c:/temp");
                Should.Throw<Exception>(() => new SystemPath("C:\\temp"));
            }
        }

        [Fact]
        public void Paths_should_compare_correctly()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                new SystemPath("file:///c:/temp").ShouldBe(new SystemPath("file:///c:/temp"));
                new SystemPath("file:///c:/temp").ShouldBe(new SystemPath("file:///c%3a/temp"));
                new SystemPath("file:///c:/temp").ShouldBe(new SystemPath("c:\\temp"));
                new SystemPath("file:///c:/temp").ShouldBe(new SystemPath("file:///C:/TEMP"));
                new SystemPath("file:///c:/temp").ShouldBe(new SystemPath("file:///C%3a/TEMP"));
                new SystemPath("file:///c:/temp").ShouldBe(new SystemPath("C:\\TEMP"));

                new SystemPath("file:///c%3a/temp").ShouldBe(new SystemPath("file:///c:/temp"));
                new SystemPath("file:///c%3a/temp").ShouldBe(new SystemPath("file:///c%3a/temp"));
                new SystemPath("file:///c%3a/temp").ShouldBe(new SystemPath("c:\\temp"));
                new SystemPath("file:///c%3a/temp").ShouldBe(new SystemPath("file:///C:/TEMP"));
                new SystemPath("file:///c%3a/temp").ShouldBe(new SystemPath("file:///C%3a/TEMP"));
                new SystemPath("file:///c%3a/temp").ShouldBe(new SystemPath("C:\\TEMP"));

                new SystemPath("C:\\temp").ShouldBe(new SystemPath("file:///c:/temp"));
                new SystemPath("C:\\temp").ShouldBe(new SystemPath("file:///c%3a/temp"));
                new SystemPath("C:\\temp").ShouldBe(new SystemPath("c:\\temp"));
                new SystemPath("C:\\temp").ShouldBe(new SystemPath("file:///C:/TEMP"));
                new SystemPath("C:\\temp").ShouldBe(new SystemPath("file:///C%3a/TEMP"));
                new SystemPath("C:\\temp").ShouldBe(new SystemPath("C:\\TEMP"));
            }
            else
            {
                new SystemPath("file:///data").ShouldBe(new SystemPath("file:///data"));
                new SystemPath("file:///data").ShouldNotBe(new SystemPath("file:///DATA"));
                new SystemPath("file:///data").ShouldBe(new SystemPath("/data"));
                new SystemPath("file:///data").ShouldNotBe(new SystemPath("/DATA"));

                new SystemPath("/data").ShouldBe(new SystemPath("file:///data"));
                new SystemPath("/data").ShouldNotBe(new SystemPath("file:///DATA"));
                new SystemPath("/data").ShouldBe(new SystemPath("/data"));
                new SystemPath("/data").ShouldNotBe(new SystemPath("/DATA"));
            }
        }
    }
}
