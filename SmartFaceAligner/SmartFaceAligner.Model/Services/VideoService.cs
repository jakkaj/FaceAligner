using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Entity;
using Contracts.Interfaces;
using XamlingCore.Portable.Contract.Config;

namespace SmartFaceAligner.Processor.Services
{
    public class VideoService : IVideoService
    {
        
        private readonly IConfigurationService _configService;
        private readonly IFileManagementService _fileManagementService;
        private readonly IFileRepo _repo;

        public VideoService(IConfigurationService configService, 
            IFileManagementService fileManagementService,
            IFileRepo repo)
        {
            _configService = configService;
            _fileManagementService = fileManagementService;
            _repo = repo;
        }
        public async Task<bool> Produce(Project project, ProjectFolderTypes folder, VideoProductionSettings settings)
        {
            var rootFolder = await _fileManagementService.GetFolder(project, folder);

            var ffMpeg = _configService.FfmpegPath;

            if (ffMpeg.ToLower().IndexOf(".exe") == -1)
            {
                ffMpeg = Path.Combine(ffMpeg, "ffmpeg.exe");
            }

            var paramsTemplate =
                "-vcodec mjpeg -framerate {0} -i img%03d.jpg -vf \"framerate=fps=30:interp_start=64:interp_end=192:scene=100\" -c:v libx264 -pix_fmt yuv420p output.mp4";

            var param = string.Format(paramsTemplate, settings.FrameRate);

            var si = new ProcessStartInfo
            {
                FileName = ffMpeg,
                Arguments = param,
                WorkingDirectory = rootFolder.FolderPath
            };

            System.Diagnostics.Process.Start(si);

            return true;
        }
    }
}
