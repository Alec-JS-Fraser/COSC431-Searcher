/*
 * Document Parser
 * Alec Fraser
 * 
 * Parser requires wsj.xml
 * 
 * Returns relevant documents with scores by calculating an AND operation.
 * This operation is done on every term recieved from stdin.
 */
using System;
using System.Text.RegularExpressions;

namespace Parser
{
    class Parser
    {
        static void Main(string[] args)
        {
            // read whole wsj file from wsj.xml
            string xml_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wsj.xml");
            string xml_contents = File.ReadAllText(xml_path); 

            // setup regex expression to identify and match to DOCNO or TEXT elements
            string extraction_regex = @"<(DOCNO|TEXT)>(.*?)<\/(DOCNO|TEXT)>";

            // match all DOCNO and TEXT elements in wsj.xml
            MatchCollection wsj_information = Regex.Matches(xml_contents, extraction_regex, RegexOptions.Singleline);

            // initialize parsed documents dictionary
            Dictionary<string, string> parsed_documents = new Dictionary<string, string>();

            // parse matched documents
            for (int i = 0; i < wsj_information.Count - 1; i += 2)
            {
                // pull DOCNO and corresponding TEXT match
                Match doc_id_match = wsj_information[i];
                Match doc_text_match = wsj_information[i+1];

                // extract the contents from DOCNO and TEXT, make text lower case
                string doc_id = doc_id_match.Groups[2].Value.Trim();
                string doc_text = doc_text_match.Groups[2].Value.Trim().ToLower();

                // split any instances of conjoined words in the text
                string joined_words_regex = @"-";
                doc_text = Regex.Replace(doc_text, joined_words_regex, " ");

                // remove any instances of punctuation or &amp; from text
                string punctuation_regex = @"\&amp;|[^\w\s]";
                doc_text = Regex.Replace(doc_text, punctuation_regex, "");

                // replace any instances of multiple spaces with a single space
                string spaces_regex = @"\s+";
                doc_text = Regex.Replace(doc_text, spaces_regex, " ");
                doc_text.Trim();

                // add document to dictionary and write to stdout
                parsed_documents.Add(doc_id, doc_text);
                Console.WriteLine("{0}: {1}\n", doc_id, doc_text);
            }

            // open and write the document dictionary to file
            using (FileStream document_stream = File.OpenWrite("document_dictionary.txt"))
            using (BinaryWriter writer = new BinaryWriter(document_stream))
            {
                writer.Write(parsed_documents.Count);

                foreach (var document in parsed_documents)
                {
                    // write parsed DOCNO and TEXT
                    writer.Write(document.Key);
                    writer.Write(document.Value);
                }
            }
        }
    }
}