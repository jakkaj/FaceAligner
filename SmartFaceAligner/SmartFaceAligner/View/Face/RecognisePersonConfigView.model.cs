using Contracts.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Interfaces;
using XamlingCore.Portable.View.Special;
using XCoreLite.View;

namespace SmartFaceAligner.View.Face
{
    public class RecognisePersonConfigViewModel : ViewModel
    {
        private readonly IFileManagementService _fileManagementService;
     
        public string GroupName { get; set; }
        public Contracts.Entity.Project Project { get; set; }


        public RecognisePersonConfigViewModel(IFileManagementService fileManagementService)
        {
            _fileManagementService = fileManagementService;
           
        }

        public async Task AddFaceToGroup(FaceData face)
        {
            //copy the face in to the new folder
            await _fileManagementService.CopyTo(face.FileName, Project, ProjectFolderTypes.RecPerson, GroupName);

        }
    }
}
