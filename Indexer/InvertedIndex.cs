/*
 * Inverted Index
 * Alec Fraser
 * 
 * Inverted index requires document_dictionary.txt to run
 * 
 * Indexer takes each document and adds each term and frequency to a sorted dictioanry of postings.
 * The sorted dictionary is then split into unique terms which points to a specific section of the index.
 * Postings are then stored in a positings lists file along with frequency.
 */
using System;

namespace Indexer
{
	public class InvertedIndex
	{
        static void Main(string[] args)
        {
            // initialize docuemnt dictionary
            Dictionary<string, string> documents = new Dictionary<string, string>();

            // read in documents
            using (FileStream doc_stream = File.OpenRead("document_dictionary.txt"))
            using (BinaryReader doc_reader = new BinaryReader(doc_stream))
            {
                int file_size = doc_reader.ReadInt32();
                
                for (int i = 0; i < file_size; i++)
                {
                    // read DOCNO and TEXT and add to dictionary
                    string doc_id = doc_reader.ReadString();
                    string text = doc_reader.ReadString().Trim();
                    documents[doc_id] = text;
                }
            }

            // initialize doc id list and the postings index
            List<string> document_ids = new List<string>();
            SortedDictionary<string, Dictionary<int, int>> posting_index = new SortedDictionary<string, Dictionary<int, int>>();
            int document_number = 0;

            // index each document
            foreach( var document in documents)
            {
                document_ids.Add(document.Key);
                string[] document_words = document.Value.Split(" ");

                // index each word
                foreach(string term in document_words)
                {
                    // check if index doesn't contain a term
                    if (!posting_index.ContainsKey(term))
                    {
                        posting_index[term] = new Dictionary<int, int>();
                    }
                    // check if term doesnt contain document number
                    if (!posting_index[term].ContainsKey(document_number))
                    {
                        posting_index[term][document_number] = 0;
                    }
                    // increment term frequency
                    posting_index[term][document_number]++;
                }
                // inceremnt doc number
                document_number++;
            }

            // initialize unique terms dictionary
            Dictionary<string, Tuple<long, int>> unique_terms = new Dictionary<string, Tuple<long, int>>();
            long location = 0;

            // open and write postings list
            using (FileStream postings_stream = File.OpenWrite("postings_list.txt"))
            using (BinaryWriter postings_writer = new BinaryWriter(postings_stream))
            {
                foreach (var posting_list in posting_index)
                {
                    // get number of postings in dictionary
                    int length = posting_list.Value.Count;

                    //create tuple of pointer location and length
                    Tuple<long, int> loc_and_len = Tuple.Create<long, int>(location, length);

                    // add term to the dictionary
                    unique_terms[posting_list.Key] = loc_and_len;

                    // update location pointer with the length of new posting
                    location += length * sizeof(int) * 2;

                    foreach(var posting in posting_list.Value)
                    {
                        // write posting doc num and frequency
                        postings_writer.Write(posting.Key);
                        postings_writer.Write(posting.Value);
                    }
                }
            }

            // open and write unique terms
            using (FileStream terms_stream = File.OpenWrite("unique_terms.txt"))
            using (BinaryWriter terms_writer = new BinaryWriter(terms_stream))
            {
                terms_writer.Write(unique_terms.Count);

                foreach (var term in unique_terms)
                {
                    // write term, pointer location and length
                    terms_writer.Write(term.Key);
                    terms_writer.Write(term.Value.Item1);
                    terms_writer.Write(term.Value.Item2);
                }
            }

            // open and write document IDs
            using (FileStream id_stream = File.OpenWrite("document_ids.txt"))
            using (BinaryWriter id_writer = new BinaryWriter(id_stream))
            {
                id_writer.Write(document_ids.Count);
                foreach(string doc_id in document_ids)
                {
                    // write DOCNO
                    id_writer.Write(doc_id);
                }
            }
        }
	}
}