using TaskManagement.Core.DTOs;
using TaskManagement.Core.Exceptions;
using TaskManagement.Services;
using Xunit;

namespace TaskManagement.Tests;

public class TaskServiceTests
{
    private readonly TaskService _service = new(TestDb.Create());

    // --- role scoping ---------------------------------------------------------

    [Fact]
    public async Task GetTasks_Admin_SeesAllTasks()
    {
        var tasks = await _service.GetTasksAsync(new TaskQuery(), viewerId: 1, isAdmin: true);

        Assert.Equal(2, tasks.Count);
    }

    [Fact]
    public async Task GetTasks_RegularUser_SeesOnlyOwnTasks()
    {
        var tasks = await _service.GetTasksAsync(new TaskQuery(), viewerId: 2, isAdmin: false);

        var task = Assert.Single(tasks);
        Assert.Equal("User's task", task.Title);
    }

    [Fact]
    public async Task GetTask_RegularUser_AskingAboutOthersTask_Throws404()
    {
        // Task 1 belongs to the admin; user id 2 must not even see it exists.
        await Assert.ThrowsAsync<ApiException>(() =>
            _service.GetTaskAsync(id: 1, viewerId: 2, isAdmin: false));
    }

    [Fact]
    public async Task GetTask_Admin_CanReadAnyTask()
    {
        var task = await _service.GetTaskAsync(id: 1, viewerId: 1, isAdmin: true);

        Assert.Equal("Admin's task", task.Title);
        Assert.Equal("Work", task.Category);          // decorated with category name
        Assert.Equal("Admin", task.AssignedTo!.Name); // and assignee name
    }

    // --- filters ----------------------------------------------------------------

    [Fact]
    public async Task GetTasks_FiltersByStatusAndPriority()
    {
        var tasks = await _service.GetTasksAsync(
            new TaskQuery { Status = "Completed", Priority = "Low" }, viewerId: 1, isAdmin: true);

        var task = Assert.Single(tasks);
        Assert.Equal("User's task", task.Title);
    }

    [Fact]
    public async Task GetTasks_Search_MatchesTitleOrDescription()
    {
        var byDescription = await _service.GetTasksAsync(
            new TaskQuery { Search = "assigned to the admin" }, viewerId: 1, isAdmin: true);
        Assert.Single(byDescription);

        var byTitle = await _service.GetTasksAsync(
            new TaskQuery { Search = "user's" }, viewerId: 1, isAdmin: true);
        Assert.Single(byTitle);

        var none = await _service.GetTasksAsync(
            new TaskQuery { Search = "zzz-no-match" }, viewerId: 1, isAdmin: true);
        Assert.Empty(none);
    }

    [Fact]
    public async Task GetTasks_SortsPendingFirst_ThenByDueDate()
    {
        var tasks = await _service.GetTasksAsync(new TaskQuery(), viewerId: 1, isAdmin: true);

        // Seed: task 1 = Pending, task 2 = Completed → Pending comes first.
        Assert.Equal("Admin's task", tasks[0].Title);
        Assert.Equal("User's task", tasks[1].Title);
    }

    // --- create -------------------------------------------------------------------

    [Fact]
    public async Task Create_EmptyTitle_Throws400()
    {
        var ex = await Assert.ThrowsAsync<ApiException>(() =>
            _service.CreateTaskAsync(new CreateTaskRequest { Title = "  " }, creatorId: 1));

        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("Title is required.", ex.Message);
    }

    [Fact]
    public async Task Create_Defaults_ToPendingAndCreator()
    {
        var task = await _service.CreateTaskAsync(
            new CreateTaskRequest { Title = "New task" }, creatorId: 1);

        Assert.Equal("Pending", task.Status);
        Assert.Equal("Medium", task.Priority);
        Assert.Equal(1, task.AssignedUserId); // unassigned → defaults to creator
    }

    [Fact]
    public async Task Create_InvalidStatus_Throws400()
    {
        var ex = await Assert.ThrowsAsync<ApiException>(() =>
            _service.CreateTaskAsync(
                new CreateTaskRequest { Title = "X", Status = "NotAStatus" }, creatorId: 1));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task Create_UnknownCategory_Throws400()
    {
        await Assert.ThrowsAsync<ApiException>(() =>
            _service.CreateTaskAsync(
                new CreateTaskRequest { Title = "X", CategoryId = 999 }, creatorId: 1));
    }

    // --- update --------------------------------------------------------------------

    [Fact]
    public async Task Update_RegularUser_EditingOthersTask_Throws403()
    {
        // Task 1 belongs to the admin; user id 2 tries to edit it.
        var ex = await Assert.ThrowsAsync<ApiException>(() =>
            _service.UpdateTaskAsync(
                id: 1, new UpdateTaskRequest { Status = "Completed" }, viewerId: 2, isAdmin: false));

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task Update_Owner_ChangesStatusAndClearsDueDate()
    {
        var task = await _service.UpdateTaskAsync(
            id: 2,
            new UpdateTaskRequest { Status = "InProgress", DueDate = "" }, // "" clears
            viewerId: 2,
            isAdmin: false);

        Assert.Equal("InProgress", task.Status);
        Assert.Null(task.DueDate);
    }

    // --- delete --------------------------------------------------------------------

    [Fact]
    public async Task Delete_RegularUser_DeletingOthersTask_Throws403()
    {
        var ex = await Assert.ThrowsAsync<ApiException>(() =>
            _service.DeleteTaskAsync(id: 1, viewerId: 2, isAdmin: false));

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task Delete_Admin_DeletesAnyTask()
    {
        await _service.DeleteTaskAsync(id: 1, viewerId: 1, isAdmin: true);

        await Assert.ThrowsAsync<ApiException>(() =>
            _service.GetTaskAsync(id: 1, viewerId: 1, isAdmin: true));
    }

    // --- dashboard stats -------------------------------------------------------------

    [Fact]
    public async Task DashboardStats_AreScopedByRole()
    {
        var admin = await _service.GetDashboardStatsAsync(viewerId: 1, isAdmin: true);
        Assert.Equal(2, admin.Total); // sees both tasks

        var user = await _service.GetDashboardStatsAsync(viewerId: 2, isAdmin: false);
        Assert.Equal(1, user.Total);    // sees only their own
        Assert.Equal(1, user.Completed);
        Assert.Equal(0, user.InProgress);
        Assert.Equal(0, user.Pending);
    }
}
