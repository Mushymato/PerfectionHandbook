namespace PerfectionHandbook.GUI.Shared;

public interface IPageContext
{
    /// <summary>
    /// Do stuff when page is set as current
    /// </summary>
    /// <returns></returns>
    bool TryOpenPage();

    /// <summary>
    /// Try to cleanup and exit this page.
    /// Return false if exiting is not possible.
    /// </summary>
    /// <returns></returns>
    bool TryExitPage();
}
