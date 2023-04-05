/*
 * Search Engine
 * Alec Fraser
 * 
 * Search engine requires 3 text files:
 * document_ids.txt
 * unique_terms.txt
 * postings_list.txt
 * 
 * Returns relevant documents with scores by calculating an AND operation.
 * This operation is done on every term recieved from stdin.
 */
using System;

namespace Searcher
{
    public class Searcher
    {
        static void Main(string[] args)
        {
            // pull all queries from stdin and add to list
            List<string> queries = new List<string>();
            string search_query;
            while((search_query = Console.ReadLine()) != null && search_query != "")
            {
                queries.Add(search_query);
            }
            
            // initilize doc id list and unique terms dictionary
            List<string> doc_ids = new List<string>();
            Dictionary<string, Tuple<long, int>> unique_terms = new Dictionary<string, Tuple<long, int>>();


            // stream in the document ids add them to the list
            using (FileStream doc_stream = File.OpenRead("document_ids.txt"))
            using (BinaryReader id_reader = new BinaryReader(doc_stream))
            {

                int file_size = id_reader.ReadInt32();

                for (int i = 0; i < file_size; i++)
                {
                    string doc_id = id_reader.ReadString();
                    doc_ids.Add(doc_id);
                }
            }

            // open and read the unique terms file
            using (FileStream terms_stream = File.OpenRead("unique_terms.txt"))
            using (BinaryReader terms_reader = new BinaryReader(terms_stream))
            {
                int file_size = terms_reader.ReadInt32();

                for (int i = 0; i < file_size; i++)
                {
                    // pull the term and corresponding pointer location and length
                    string term = terms_reader.ReadString();
                    long location = terms_reader.ReadInt64();
                    int length = terms_reader.ReadInt32();

                    // add term to the dictionary
                    unique_terms[term] = Tuple.Create<long, int>(location, length);
                }
            }

            //open the posting list to read in document numbers and frequencies
            using (FileStream postings_stream = File.OpenRead("postings_list.txt"))
            using (BinaryReader postings_reader = new BinaryReader(postings_stream))
            {
                foreach (string query in queries)
                {
                    string[] terms = query.Split(" ");

                    // create a hashset to hold all relevant documents
                    HashSet<int> docs_to_score = new HashSet<int>();
                    bool first_set = true;

                    // initialize term frequency dictionary
                    Dictionary<string, Dictionary<int, int>> term_frequencies = new Dictionary<string, Dictionary<int, int>>();

                    foreach (string term in terms)
                    {

                        if (unique_terms.ContainsKey(term))
                        {
                            // create term hash set
                            HashSet<int> doc_nums = new HashSet<int>();

                            // set posting stream pointer to term location
                            postings_stream.Seek(unique_terms[term].Item1, SeekOrigin.Begin);

                            // for term posting length get the doc nums and freqs
                            for (int i = 0; i < unique_terms[term].Item2 - 1; i++)
                            {
                                // read doc num and frequency
                                int doc_num = postings_reader.ReadInt32();
                                int doc_freq = postings_reader.ReadInt32();

                                // add doc num to the hash set
                                doc_nums.Add(doc_num);
                                // add the term frequency to the dictionary
                                if (!term_frequencies.ContainsKey(term))
                                {
                                    term_frequencies[term] = new Dictionary<int, int>();
                                }
                                if (!term_frequencies[term].ContainsKey(doc_num))
                                {
                                    term_frequencies[term][doc_num] = doc_freq;
                                }
                            }

                            // if the hash set is the first add the whole set
                            if (first_set)
                            {
                                docs_to_score.UnionWith(doc_nums);
                                first_set = false;
                            }
                            else // otherwise docs must intersect with eachother (performs AND)
                            {
                                docs_to_score.IntersectWith(doc_nums);
                            }
                        }
                    }
                
                    // initialize list of scored documents
                    List<(int, double)> doc_scores = new List<(int, double)>();

                    // compute documents TF-IDF
                    foreach (int doc_num in docs_to_score)
                    {
                        double doc_score = 0.0;
                        foreach (string term in terms)
                        {
                            if (unique_terms.ContainsKey(term))
                            {
                                double term_frequency = 0.0;
                                // check if term is held in dictonary 
                                try
                                {
                                    term_frequency = (double)term_frequencies[term][doc_num];
                                }
                                catch (KeyNotFoundException)
                                {
                                }
                                // calculate document frequency
                                double doc_frequency = ((double)unique_terms[term].Item2 / (double)doc_ids.Count);

                                // sum the scores for each term
                                doc_score += term_frequency * (1.0 / doc_frequency);
                            }
                        }
                        // add TF-IDF to scores list
                        doc_scores.Add((doc_num, doc_score));
                    }

                    // compare and sort doc scores 
                    doc_scores.Sort((doc1, doc2) => doc2.Item2.CompareTo(doc1.Item2));

                    // print DOCNO and relevancy in descending order
                    foreach ((int, double) doc in doc_scores)
                    {
                        Console.WriteLine("{0} {1}", doc_ids[doc.Item1], doc.Item2);
                    }
                    Console.WriteLine();
                    // clear dictionaries
                    doc_scores.Clear();
                    docs_to_score.Clear();
                    term_frequencies.Clear();
                }
            }
        }
    }
}