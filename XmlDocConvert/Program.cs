using System;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace XmlDocConvert
{

    sealed class ProjectMember
    {
        public string Role { get; private set; }
        public string Name { get; private set; }
        public ProjectMember(string role, string name)
        {
            this.Role = role ?? string.Empty;
            this.Name = name ?? string.Empty;
        }
    }

    sealed class Project
    {
        public string Name { get; private set; }
        public ReadOnlyCollection<ProjectMember> Members { get; private set; }
        public Project(string name, IList<ProjectMember> members)
        {
            if ((object)members != null && members.Contains(null))
                throw new ArgumentException("Member list must contains all not null values.", "members");
            this.Name = name ?? string.Empty;
            this.Members = new ReadOnlyCollection<ProjectMember>(members ?? new ProjectMember[0]);
        }
    }

    sealed class MemberRole
    {
        public string Name { get; private set; }
        public string Project { get; private set; }
        public MemberRole(string name, string project)
        {
            this.Name = name ?? string.Empty;
            this.Project = project ?? string.Empty;
        }
    }

    sealed class Member
    {
        public string Name { get; private set; }
        public ReadOnlyCollection<MemberRole> Roles { get; private set; }
        public Member(string name, IList<MemberRole> roles)
        {
            if ((object)roles != null && roles.Contains(null))
                throw new ArgumentException("Role list must contains all not null values.", "roles");
            this.Name = name ?? string.Empty;
            this.Roles = new ReadOnlyCollection<MemberRole>(roles ?? new MemberRole[0]);
        }
    }

    sealed class XmlDocConverter
    {
        private static class ProjectConsts
        {
            public const string ProjectsHeader = "<projects>";
            public const string ProjectsTail = "</projects>";

            public const string ProjectHeaderStart = "    <project name=\"";
            public const string ProjectHeaderEnd = "\">";
            public static readonly int ProjectHeaderMinLength = ProjectHeaderStart.Length + ProjectHeaderEnd.Length;
            public const string ProjectHeaderTemplate = ProjectHeaderStart + "{0}" + ProjectHeaderEnd;
            public const string ProjectTail = "    </project>";

            public const string ProjectMemberStart = "        <member role=\"";
            public const string ProjectMemberEnd = "\"/>";
            public const string ProjectMemberNameStart = "\" name=\"";
            public const string ProjectMemberNameTemplate = ProjectMemberStart + "{0}" + ProjectMemberNameStart + "{1}" + ProjectMemberEnd;

            public static readonly int ProjectMemberMinLength = ProjectMemberStart.Length + ProjectMemberNameStart.Length + ProjectMemberEnd.Length;

            static ProjectConsts() { }
        }

        private readonly Func<string> input;
        private readonly Action<string> output;
        private readonly StringComparer nameComparer = StringComparer.InvariantCulture;

        public XmlDocConverter(Func<string> input, Action<string> output)
        {
            this.input = input ?? Console.ReadLine;
            this.output = output ?? Console.WriteLine;
        }

        public IList<Project> ReadProjects()
        {
            if (this.input() != ProjectConsts.ProjectsHeader)
                throw new ArgumentException("Projects header was expected.");

            List<Project> inputProjects = new List<Project>();
            string projectHeader;
            while ((projectHeader = this.input()) != ProjectConsts.ProjectsTail)
            {
                if (!(
                    projectHeader.Length >= ProjectConsts.ProjectHeaderMinLength &&
                    projectHeader.StartsWith(ProjectConsts.ProjectHeaderStart, StringComparison.Ordinal) &&
                    projectHeader.EndsWith(ProjectConsts.ProjectHeaderEnd, StringComparison.Ordinal)
                ))
                    throw new ArgumentException("Project header is incorrect.");

                string projectName = projectHeader.Substring(
                    ProjectConsts.ProjectHeaderStart.Length,
                    projectHeader.Length - (ProjectConsts.ProjectHeaderStart.Length + ProjectConsts.ProjectHeaderEnd.Length)
                );

                List<ProjectMember> projectMembers = new List<ProjectMember>();
                string memberLine;
                while ((memberLine = this.input()) != ProjectConsts.ProjectTail)
                {
                    if (!(
                        memberLine.Length >= ProjectConsts.ProjectMemberMinLength &&
                        memberLine.StartsWith(ProjectConsts.ProjectMemberStart, StringComparison.Ordinal) &&
                        memberLine.EndsWith(ProjectConsts.ProjectMemberEnd, StringComparison.Ordinal) &&
                        memberLine.Contains(ProjectConsts.ProjectMemberNameStart)
                    ))
                        throw new ArgumentException("Member header is incorrect.");

                    int indexOfProjectMemberNameStart = memberLine.IndexOf(ProjectConsts.ProjectMemberNameStart, StringComparison.Ordinal);
                    int memberRoleLength = indexOfProjectMemberNameStart - ProjectConsts.ProjectMemberStart.Length;
                    string memberRole = memberLine.Substring(ProjectConsts.ProjectMemberStart.Length, memberRoleLength);

                    int indexOfProjectMemberEnd = memberLine.IndexOf(ProjectConsts.ProjectMemberEnd, StringComparison.Ordinal);
                    int indexOfProjectMemberName = ProjectConsts.ProjectMemberStart.Length + memberRole.Length + ProjectConsts.ProjectMemberNameStart.Length;
                    int memberNameLength = indexOfProjectMemberEnd - indexOfProjectMemberName;
                    string memberName = memberLine.Substring(indexOfProjectMemberName, memberNameLength);

                    projectMembers.Add(new ProjectMember(memberRole, memberName));
                } // while (member list)

                inputProjects.Add(new Project(projectName, projectMembers));
            } // while (project list)

            return inputProjects;
        }

        public void WriteProjects(IList<Project> projects)
        {
            this.output(ProjectConsts.ProjectsHeader);

            for (int i = 0; i < projects.Count; i++)
            {
                this.output(string.Format(CultureInfo.InvariantCulture, ProjectConsts.ProjectHeaderTemplate, projects[i].Name));
                for (int j = 0; j < projects[i].Members.Count; j++)
                    this.output(string.Format(CultureInfo.InvariantCulture, ProjectConsts.ProjectMemberNameTemplate, projects[i].Members[j].Role, projects[i].Members[j].Name));
                this.output(ProjectConsts.ProjectTail);
            }

            this.output(ProjectConsts.ProjectsTail);
        }

        private IEnumerable<Member> ConvertInternal(IList<Project> projects)
        {
            var memberNames = projects.SelectMany(project => project.Members.Select(member => member.Name)).Distinct();
            foreach (string memberName in memberNames)
            {
                yield return new Member(memberName, null);
            }
        }

        public IList<Member> Convert(IList<Project> projects)
        {
            return this.ConvertInternal(projects).OrderBy(member => member.Name, this.nameComparer).ToList();
        }

        public void WriteMembers(IList<Member> members)
        {
            this.output("<members>");
            for (int i = 0; i < members.Count; i++)
            {
                this.output(string.Format(
                    CultureInfo.InvariantCulture,
                    "    <member name=\"{0}\"/>",
                    members[i].Name
                ));
                for (int j = 0; j < members[i].Roles.Count; j++)
                {
                    this.output(string.Format(
                        CultureInfo.InvariantCulture,
                        "        <role name=\"{0}\" project=\"{1}\"/>",
                        members[i].Roles[j].Name,
                        members[i].Roles[j].Project
                    ));
                }
                this.output("    </member>");
            }
            this.output("</members>");
        }
    }

    static class Program
    {
        static void Main(string[] args)
        {
            XmlDocConverter converter = new XmlDocConverter(null, null);
            IList<Project> inputProjects = converter.ReadProjects();

            IList<Member> members = converter.Convert(inputProjects);
            converter.WriteMembers(members);

            Console.ReadLine();
        }
    }

}
