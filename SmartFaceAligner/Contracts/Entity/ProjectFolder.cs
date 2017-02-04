using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Entity
{
    public enum ProjectFolderTypes
    {
        Staging,
        Filtered,
        Aligned
    }

    public class ProjectFolder
    {
        public ProjectFolderTypes ProjectFolderType { get; set; }
        public Project Project { get; set; }
        public string FolderPath { get; set; }
    }
}
