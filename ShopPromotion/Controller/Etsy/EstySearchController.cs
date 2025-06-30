using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using OpenQA.Selenium;

namespace ShopPromotion.Controller.Etsy
{
    class SearchController : Controller, IShopSearchController
    {
        public void SearchText(string text)
        {
            IWebElement mainSearchField = Driver.FindElement(ByAttribute.Value("id", "global-enhancements-search-query", "input"));

            mainSearchField.Clear();

            mainSearchField.SendKeys(text);
            mainSearchField.SendKeys(Keys.Enter);
        }

        public List<IWebElement> GetListOfSearchResults()
        {
            return GetSearchResultContext().FindElements(By.ClassName("js-merch-stash-check-listing")).ToList();
        }

        /// <exception cref="T:OpenQA.Selenium.NoSuchElementException">If no element matches the criteria.</exception>
        public IWebElement FindListingInSearchResults(string listingId)
        {
            return GetSearchResultContext().FindElement(ByAttribute.Value("data-listing-id", listingId));
        }

        public bool OpenNextSearchPage(int nextPageNumber)
        {
            ISearchContext searchPagesGroup;
            try
            {
                searchPagesGroup = Driver.FindElement(ByAttribute.Name("data-search-pagination", "div"));
            }
            catch (NoSuchElementException)
            {
                return false;
            }

            List<IWebElement> switchPageButtonGroups = searchPagesGroup.FindElements(By.ClassName("wt-action-group__item-container")).ToList();
            if (!switchPageButtonGroups.Any())
                return false;

            for (int index = switchPageButtonGroups.Count - 1; index >= 0; --index)
            {
                var pageGroup = switchPageButtonGroups[index];
                var buttonLink = pageGroup.GetElementLink();

                if (string.IsNullOrEmpty(buttonLink))
                    continue;

                try
                {
                    // example: data-href=https://www.etsy.com/search?q=%SEARCH_TEXT%&ref=pagination%SOMETHING%&page=%PAGE_NUMBER% try extract PAGE_NUMBER
                    var match = Regex.Match(buttonLink, "etsy.com/search.*ref=pagination.*&page=(.*)");
                    var pageNumber = int.Parse(match.Groups[1].Value);
                    if (nextPageNumber != pageNumber)
                        continue;

                    ScrollToElement(pageGroup);
                    pageGroup.Click();
                    return true;
                }
                catch (Exception exception)
                {
                    if (!(exception is RegexMatchTimeoutException ||
                          exception is ArgumentException ||
                          exception is FormatException ||
                          exception is OverflowException ||
                          exception is WebDriverException))
                        throw;

                    Debug.Assert(false);
                }
            }

            return false;
        }

        private ISearchContext GetSearchResultContext()
        {
            ISearchContext allResultsTable = Driver;

            try
            {
                allResultsTable = allResultsTable.FindElement(ByAttribute.Name("data-search-results", "div"));
            }
            catch (NoSuchElementException) { }

            try
            {
                allResultsTable = allResultsTable.FindElement(ByAttribute.Name("data-search-results-lg", "div"));
            }
            catch (NoSuchElementException) { }

            return allResultsTable;
        }
    }
}
