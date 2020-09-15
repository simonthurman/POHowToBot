using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;

namespace POHowToBot.Dialogs
{
    public class RootDialog : ComponentDialog
    {
        /// <summary>
        /// QnA Maker initial dialog
        /// </summary>
        private const string InitialDialog = "initial-dialog";

        /// <summary>
        /// Initializes a new instance of the <see cref="RootDialog"/> class.
        /// </summary>
        /// <param name="services">Bot Services.</param>
        public RootDialog(IBotService services, IConfiguration configuration)
            : base("root")
        {
            AddDialog(new QnAMakerBaseDialog(services, configuration));

            AddDialog(new WaterfallDialog(InitialDialog)
               .AddStep(InitialStepAsync));

            // The initial child Dialog to run.
            InitialDialogId = InitialDialog;
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(QnAMakerDialog), null, cancellationToken);
        }
    }
}
