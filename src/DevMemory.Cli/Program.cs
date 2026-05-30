using DevMemory.Application;
using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands;
using DevMemory.Cli.Commands.Ai;
using DevMemory.Cli.Commands.Git;
using DevMemory.Cli.Commands.Graph;
using DevMemory.Cli.Commands.Memory;
using DevMemory.Cli.Commands.System;
using DevMemory.Infrastructure;
using DevMemory.Infrastructure.Git;
using DevMemory.Infrastructure.Graph;
using DevMemory.Infrastructure.Markdown;
using DevMemory.Infrastructure.Storage;


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
    new EditCommandHandler(memoryService),
    new DeleteCommandHandler(memoryService),
    new TimelineCommandHandler(memoryService),
    new StorageCommandHandler(memoryService),
    new MarkdownCommandHandler(memoryService),
    new GitStatusCommandHandler(gitInspector),
    new LearnFromGitCommandHandler(memoryService, gitMemoryDraftService),
    new GraphExportCommandHandler(memoryGraphService),
    new GraphViewCommandHandler(memoryGraphService),
    new AiStatusCommandHandler(),
    new AiDoctorCommandHandler(),
    new AskCommandHandler(),
    new IndexCommandHandler(memoryService),
    new RelatedCommandHandler(memoryService),
    new SemanticSearchCommandHandler(),
    new ConfigCommandHandler(),
    new VersionCommandHandler(),
    new HelpCommandHandler()
};

var dispatcher = new CommandDispatcher(commandHandlers);

return dispatcher.Dispatch(args);
