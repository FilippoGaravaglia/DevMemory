using DevMemory.Application;
using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands;
using DevMemory.Infrastructure;
using DevMemory.Infrastructure.Git;
using DevMemory.Infrastructure.Graph;

var repository = new MemoryRepository();
var markdownExporter = new MarkdownMemoryExporter();
var memoryService = new MemoryService(repository, markdownExporter);

var gitInspector = new GitRepositoryInspector();
var gitMemoryDraftService = new GitMemoryDraftService(gitInspector);

var graphExporter = new JsonMemoryGraphExporter();
var graphHtmlExporter = new HtmlMemoryGraphExporter();
var memoryGraphService = new MemoryGraphService(repository, graphExporter, graphHtmlExporter);

var commandHandlers = new ICommandHandler[]
{
    new AddCommandHandler(memoryService),
    new ListCommandHandler(memoryService),
    new SearchCommandHandler(memoryService),
    new ShowCommandHandler(memoryService),
    new StorageCommandHandler(memoryService),
    new MarkdownCommandHandler(memoryService),
    new GitStatusCommandHandler(gitInspector),
    new LearnFromGitCommandHandler(memoryService, gitMemoryDraftService),
    new GraphExportCommandHandler(memoryGraphService),
    new GraphViewCommandHandler(memoryGraphService),
    new HelpCommandHandler()
};

var dispatcher = new CommandDispatcher(commandHandlers);

return dispatcher.Dispatch(args);
