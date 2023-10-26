/*
 * Class Name: AssignmentTrialOne.cs
 * Author: Zaid Shaikh
 * Definition: 
 *  - This class reads a particular directory and scans each and every folder for csv files.
 *  - From these csv files it gets the data and checks for valid rows. Once the rows are valid, it copies the row to 
 *  a Output.csv file. 
 *  - If the data is invalid or an exception is thrown, both are caught and logged into a log file.
*/

using System;
using System.IO;
using CsvHelper;	//To use the csvhelper class
using System.Globalization;
using CsvHelper.Configuration;
using System.Linq;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;	//to map the property names with the actual column names in the csv file
using System.Diagnostics;   // to calculate run time
using Serilog;  // to handle logging in the code.
using Serilog.Sinks.File;   // to handle writing the on the log information
using System.Text.RegularExpressions;



namespace proj1
{
    //the class contains main method for now that is directly reading the file now using csvhelper.
    public class AssignmentTrialOne
    {
        //method to check null or empty
        public static Boolean isNull(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //creating a new file for Logging. Keeping the interval to a day to create file each day.
            Log.Logger = new LoggerConfiguration().WriteTo.File("log.txt", rollingInterval: RollingInterval.Day).CreateLogger();
            int totalNumberofValidRows = 0;
            int totalNumberofInvalidRows = 0;
            bool headers = true;
            try
            {

                string directoryPath = "/Users/zaidshaikh/Documents/Assignments/Software Dev/Sample Data";    //The path to the directory containing data
                string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);   //Will make sure the subdirectories are read for files.
                string filePathForOutputCSV = "/Users/zaidshaikh/Documents/Assignments/Software Dev/output.csv";

                //iterating over the paths to read each and every file
                foreach (string filePath in files)
                {

                    //Code to Extract Date Using filePath
                    string Date = "";
                    // Regular expression pattern to match the date format in the path
                    string pattern = @"/(\d{4})/(\d{1,2})/(\d{1,2})/";
                    // Match the pattern using regular expressions
                    Match match = Regex.Match(filePath, pattern);
                    if (match.Success && match.Groups.Count == 4)
                    {
                        // Extract year, month, and day from the matched groups
                        string year = match.Groups[1].Value;
                        string month = match.Groups[2].Value;
                        string day = match.Groups[3].Value;

                        // Create the date string in the format "yyyy-MM-dd"
                        Date = $"{year}/{month}/{day}";
                    }
                    else
                    {
                        Log.Information($"Date is not calculated here : {filePath}");
                        Console.WriteLine($"Date is not Calculated here: {Date}, Path: {filePath}");
                    }
                    //Code end to extract date
                    Console.WriteLine(filePath);    // added to check where the code is going
                                                    //Here we have using that helps to read the file via StreamReader method.


                    using (var streamReader = new StreamReader(filePath))
                    {
                        //using the csvHelper library method and properties here trying to read the lines in csv file.
                        //The InvariantCulture is the instance of CultureInfor class that determines that standard formatting is to be used.
                        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                        {

                            HasHeaderRecord = false, // Skip headers while reading
                            MissingFieldFound = null,   // Even if the file has some missing values, ignore it and not throw any error.
                            BadDataFound = null     // if the csv file has missing commas based on the declared header class, it counts as bad data. This helps tp skip thpse records and keep moving

                        };
                        using (var csvReader = new CsvReader(streamReader, config))
                        using (var writer = new StreamWriter(filePathForOutputCSV, append: true))
                        using (var csvWriter = new CsvWriter(writer, config))
                        {
                            //check if the file is present on the path or newly created, if not then wrtites the header row.
                            //if (!File.Exists(filePathForOutputCSV))
                            if (headers)
                            {
                                csvWriter.WriteHeader<CustomerDataHeaders>();
                                //csvWriter.WriteHeader("First Name","Last Name","");
                                headers = false;
                                csvWriter.NextRecord();
                                //headers = false;
                                Console.WriteLine("Entered Once in this Loop");
                                Log.Information("Entered if condition once first itme");
                            }
                            int rowInFile = 1;  // to calculate the invalid row number in the partocular file 
                            csvReader.Read();   // This skips the first Line of every csv file. 
                            var records = csvReader.GetRecords<CustomerDataHeaders>().ToList(); // Using the map class to check on the propper headernames as per csv files.
                            foreach (var rec in records)
                            {
                                
                                if (isNull(rec.FirstName) || isNull(rec.Lastname) || isNull(rec.Street) || isNull(rec.City) ||
                                    isNull(rec.Country) || isNull(rec.Province) || isNull(rec.PostalCode) || isNull(rec.PhoneNumber) || isNull(rec.StreetNumber))
                                {

                                    //Console.WriteLine("Invalid Row: " + (rowNumber + 1));
                                    totalNumberofInvalidRows++;
                                    Log.Information($"Invalid Data, Row Skipped. File: {filePath}, Row Number: {rowInFile}");

                                }
                                else
                                {
                                    totalNumberofValidRows++;
                                    rec.Date = Date;
                                    // Write the updated record to the output CSV file
                                    csvWriter.WriteRecord(rec);
                                    //csvWriter.WriteField(Date); trying tio add date directly to rec
                                    csvWriter.NextRecord();
                                }
                                rowInFile++;
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("There is an error: " + e);
                Log.Error(e.ToString());
            }
            stopwatch.Stop();
            TimeSpan readTime = stopwatch.Elapsed;
            Log.Information($"Total time taken for execution: {readTime}");
            Log.Information($"Total number of Valid Rows scanned: {totalNumberofValidRows}"); ;
            Log.Information($"Total number of Skipped rows: {totalNumberofInvalidRows}");
            Console.WriteLine($"Total time taken for execution: {readTime}");
        }
    }

    //In the below class we declare the header column names in our csv file. This will be used as the map class for the GetRecords method of csvreader class
    public class CustomerDataHeaders
    {
        [Name("First Name")]
        public string FirstName { get; set; }
        [Name("Last Name")]
        public string Lastname { get; set; }
        [Name("Street Number")]
        public string StreetNumber { get; set; }
        [Name("Street")]
        public string Street { get; set; }
        [Name("City")]
        public string City { get; set; }
        [Name("Province")]
        public string Province { get; set; }
        [Name("Postal Code")]
        public string PostalCode { get; set; }
        [Name("Country")]
        public string Country { get; set; }
        [Name("Phone Number")]
        public string PhoneNumber { get; set; }
        [Name("email Address")]
        public string Email { get; set; }
        [Name("Date")]
        public string Date { get; set; }

    }


}


