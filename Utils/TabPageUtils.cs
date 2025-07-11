namespace Avatar_Explorer.Utils;

internal static class TabPageUtils
{
    private const int HorizontalMargin = 25;

    internal static readonly Size ButtonSize = new(50, 40);
    internal static readonly Size SmallButtonSize = new(40, 30);

    internal const int ButtonSpacing = 10;
    internal const int SmallButtonSpacing = 3;

    internal static void AddNavigationButtons(
        TabPage tabPage,
        int baseYLocation,
        int pageCount, int itemsPerPage, int totalCount, bool small,
        string currentLanguage,
        EventHandler? onBackClick,
        EventHandler? onNextClick,
        EventHandler? onFirstClick,
        EventHandler? onLastClick,
        EventHandler? Reload
    )
    {
        int totalPages = (int)Math.Ceiling((double)totalCount / itemsPerPage);
        int start = pageCount * itemsPerPage + 1;
        int end = Math.Min(start + itemsPerPage - 1, totalCount);

        bool enableFirstButton = pageCount != 0;
        bool enableBackButton = pageCount != 0;
        bool enableNextButton = pageCount < totalPages - 1;
        bool enableLastButton = pageCount < totalPages - 1;

        Label pageInfoLabel = new()
        {
            Text = $"{pageCount + 1} / {totalPages}{LanguageUtils.Translate("ページ", currentLanguage)}\n{start} - {end} / {totalCount}{LanguageUtils.Translate("個の項目", currentLanguage)}",
            Name = "PageInfoLabel",
            Font = small ? new Font("Yu Gothic UI", 9.2F) : new Font("Yu Gothic UI", 12F),
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter
        };

        Size labelSize = TextRenderer.MeasureText(pageInfoLabel.Text, pageInfoLabel.Font);
        pageInfoLabel.Location = GetLabelLocation(tabPage.Width, labelSize, baseYLocation);

        if (enableFirstButton || enableBackButton || enableNextButton || enableLastButton)
        {
            tabPage.Controls.Add(pageInfoLabel);
        }

        if (enableFirstButton)
        {
            Button firstButton = new()
            {
                Text = "<<",
                Name = "FirstPageButton",
                Size = small ? SmallButtonSize : ButtonSize,
                Location = GetFirstButtonLocation(tabPage.Width, labelSize.Width, baseYLocation, small),
                Font = small ? new Font("Yu Gothic UI", 10F) : new Font("Yu Gothic UI", 13F)
            };

            if (onFirstClick != null)
            {
                firstButton.Click += onFirstClick;
                firstButton.Click += Reload;
            }

            tabPage.Controls.Add(firstButton);
        }

        if (enableBackButton)
        {
            Button backButton = new()
            {
                Text = "<",
                Name = "BackPageButton",
                Size = small ? SmallButtonSize : ButtonSize,
                Location = new Point(GetFirstButtonLocation(tabPage.Width, labelSize.Width, baseYLocation, small).X + (small ? SmallButtonSpacing + SmallButtonSize.Width : ButtonSpacing + ButtonSize.Width), baseYLocation),
                Font = small ? new Font("Yu Gothic UI", 10F) : new Font("Yu Gothic UI", 13F)
            };

            if (onBackClick != null)
            {
                backButton.Click += onBackClick;
                backButton.Click += Reload;
            }

            tabPage.Controls.Add(backButton);
        }

        if (enableLastButton)
        {
            Button lastButton = new()
            {
                Text = ">>",
                Name = "LastPageButton",
                Size = small ? SmallButtonSize : ButtonSize,
                Location = GetLastButtonLocation(tabPage.Width, labelSize.Width, baseYLocation, small),
                Font = small ? new Font("Yu Gothic UI", 10F) : new Font("Yu Gothic UI", 13F)
            };

            if (onLastClick != null)
            {
                lastButton.Click += onLastClick;
                lastButton.Click += Reload;
            }

            tabPage.Controls.Add(lastButton);
        }

        if (enableNextButton)
        {
            Button nextButton = new()
            {
                Text = ">",
                Name = "NextPageButton",
                Size = small ? SmallButtonSize : ButtonSize,
                Location = new Point(GetLastButtonLocation(tabPage.Width, labelSize.Width, baseYLocation, small).X - (small ? SmallButtonSpacing + SmallButtonSize.Width : ButtonSpacing + ButtonSize.Width), baseYLocation),
                Font = small ? new Font("Yu Gothic UI", 10F) : new Font("Yu Gothic UI", 13F),
            };

            if (onNextClick != null)
            {
                nextButton.Click += onNextClick;
                nextButton.Click += Reload;
            }

            tabPage.Controls.Add(nextButton);
        }

        pageInfoLabel.SendToBack();
    }

    internal static Point GetFirstButtonLocation(int tabWidth, int labelWidth, int baseYLocation = 0, bool small = false)
    {
        int buttonSpacing = small ? SmallButtonSpacing : ButtonSpacing;
        int buttonWidth = small ? SmallButtonSize.Width : ButtonSize.Width;

        int x = (tabWidth / 4) - buttonWidth;
        int backButtonX = x + buttonWidth + buttonSpacing;
        int labelX = (tabWidth / 2) - (labelWidth / 2);

        if (backButtonX + buttonWidth > labelX)
        {
            x = labelX - (buttonWidth * 2) - buttonSpacing;
        }

        x = Math.Max(x, HorizontalMargin);

        return new(x, baseYLocation);
    }

    internal static Point GetLastButtonLocation(int tabWidth, int labelWidth, int baseYLocation = 0, bool small = false)
    {
        int buttonSpacing = small ? SmallButtonSpacing : ButtonSpacing;
        int buttonWidth = small ? SmallButtonSize.Width : ButtonSize.Width;

        int x = tabWidth - (tabWidth / 4);
        int nextButtonX = x - buttonWidth - buttonSpacing;
        int labelX = (tabWidth / 2) + (labelWidth / 2);

        if (nextButtonX < labelX)
        {
            x = labelX + buttonWidth + buttonSpacing;
        }

        x = Math.Min(x, tabWidth - HorizontalMargin - buttonWidth);

        return new(x, baseYLocation);
    }

    internal static Point GetLabelLocation(int tabWidth, Size labelSize, int baseYLocation = 0)
        => new((tabWidth / 2) - (labelSize.Width / 2), baseYLocation + 20 - (labelSize.Height / 2) - 5);
}