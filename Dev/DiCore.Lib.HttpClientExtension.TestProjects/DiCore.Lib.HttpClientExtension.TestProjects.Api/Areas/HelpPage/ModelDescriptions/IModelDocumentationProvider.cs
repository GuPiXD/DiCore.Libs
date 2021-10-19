using System;
using System.Reflection;

namespace DiCore.Lib.HttpClientExtension.TestProjects.Api.Areas.HelpPage.ModelDescriptions
{
    public interface IModelDocumentationProvider
    {
        string GetDocumentation(MemberInfo member);

        string GetDocumentation(Type type);
    }
}