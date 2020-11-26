using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbfTests
{
    [TestClass]
    public class DbfTestTask
    {
        [TestMethod]
        public void TestTask()
        {
            const string RootDir = @".\Data";
            const string RelevantFileName = "128.dbf";
            var reader = new DbfReader();

            // TODO read all RelevantFileName files recursively from RootDir (will be copied on build)
            // use DbfReader to read them and extract all DataValues
            // here an example call for one file:
            //var values = reader.ReadValues(@".\Data\ELEKTRO\E01\E600DI01\128.dbf");


            // get all the files from the root dir
            var allFiles = Directory.GetFiles(RootDir, $"{RelevantFileName}", SearchOption.AllDirectories).ToList();

            //get the raw values from files before processing
            var rawValues = allFiles
                .SelectMany(file => reader.ReadValues(file));


            // put all DataValues into ONE ordered (by timestamp) list of OutputRow (each timestamp shall exist only once)
            // the OutputRow has 2 lists: 1 static one for the headers (directory path of file) and one for the values (values of all files (same timestamp) must be merged into one OutputRow)

            var outputs = rawValues.GroupBy(x => x.Timestamp)
                .OrderBy(x => x.Key)
                .Select(output => new OutputRow
                {
                    Timestamp = output.Key,
                    Values = output.Select(x => (double?)x.Value).ToList()
                })
                .ToList();

            /* 
             * filling any output value to match the number of files            
            */

            foreach (var item in outputs)
            {
                var valueCount = item.Values.Count;
                if (valueCount <= allFiles.Count)
                {
                    var repeated = Enumerable.Repeat(default(double?), allFiles.Count - valueCount);
                    item.Values.AddRange(repeated);
                }
            }

            OutputRow.Headers = allFiles;
            // if there is time left, improve example where you think it isn't good enough

            // the following asserts should pass
            Assert.AreEqual(27, outputs[0].Values.Count);
            Assert.AreEqual(27, outputs[11110].Values.Count);
            Assert.AreEqual(27, outputs[25789].Values.Count);
            Assert.AreEqual(25790, outputs.Count);
            Assert.AreEqual(27, OutputRow.Headers.Count);
            Assert.AreEqual(633036852000000000, outputs.Min(o => o.Timestamp).Ticks);
            Assert.AreEqual(634756887000000000, outputs.Max(o => o.Timestamp).Ticks);
            Assert.AreEqual(633036852000000000, outputs[0].Timestamp.Ticks);
            Assert.AreEqual(634756887000000000, outputs.Last().Timestamp.Ticks);

            // write into file that we can compare results later on (you don't have to do something)
            string content = "Time\t" + string.Join("\t", OutputRow.Headers) + Environment.NewLine +
                          string.Join(Environment.NewLine, outputs.Select(o => o.AsTextLine()));
            File.WriteAllText(@".\output.txt", content);
        }
    }
}
