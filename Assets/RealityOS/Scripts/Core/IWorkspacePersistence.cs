using System.Collections.Generic;
using RealityOS.Persistence;

namespace RealityOS.Core
{
    public interface IWorkspacePersistence
    {
        bool SaveWorkspace(WorkspaceData workspace);
        WorkspaceData LoadWorkspace(string workspaceId);
        bool DeleteWorkspace(string workspaceId);
        List<WorkspaceData> GetAllWorkspaces();
        bool WorkspaceExists(string workspaceId);
    }
}

