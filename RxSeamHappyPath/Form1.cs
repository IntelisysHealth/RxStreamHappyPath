using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using RestSharp;

namespace RxSeamHappyPath
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            //reset the form
            lblOutput.Text = "";
            txtJson.Text = "";
            gridJson.DataSource = null;

            if (ValidAppConfig())
            {
                //Call RxStream Asyc so we don' lock the form
                CallRxStreamAsync();
            }
        }

        private bool ValidAppConfig()
        {
            bool valid = true;
            string url = ConfigurationSettings.AppSettings["url"].ToString();
            string tenantId = ConfigurationSettings.AppSettings["tenantId"].ToString();
            string apiKey = ConfigurationSettings.AppSettings["apiKey"].ToString();

            if (url == "http://XXXX/api/")
                valid = false;

            if (tenantId == "XXXX")
                valid = false;

            if (apiKey == "XXXX")
                valid = false;

            if (!valid)
                lblOutput.Text =  "Please check App.Config";


            return valid;
        }

        private async void CallRxStreamAsync()
        {
            //Get the config values
            lblOutput.Text = "processing...";
            string url = ConfigurationSettings.AppSettings["url"].ToString();
            string tenantId = ConfigurationSettings.AppSettings["tenantId"].ToString();
            string apiKey = ConfigurationSettings.AppSettings["apiKey"].ToString();

            //get the methods you are going to call
            var client = new RestClient(url);
            var request = new RestRequest("Prescription/ClosestEstimate", Method.POST);

            //This is another method API you can call and the inputs are he same but you should deserialized to a collection of EstimateDto
            //var request = new RestRequest("Prescription/Estimate", Method.POST);


            //Add the keys to the request header, these are requred for ever call
            request.AddHeader("TenantId", tenantId);
            request.AddHeader("ApiKey", apiKey);

            //Start building the Prescripton Dto 

            //put together the header with the minimal info
            var prescriptionDto = new PrescriptionDto
            {
                ClientId = Guid.NewGuid().ToString(),
                PatZip = "78727",
                DocZip = "90210",
                PharmacyId = "1508965765,1689000606"
            };

            //Create / Add a prescription line item
            prescriptionDto.LineItems.Add(new PrescriptionItemDto
            {
                PrescriptionNumber = Guid.NewGuid().ToString(), //Normally this would come from the client and you would use it to match back in the response
                Ndc = "39822015101",
                Quantity = 30
            });


            //Add the Json body
            var json = JsonConvert.SerializeObject(prescriptionDto);
            request.AddParameter("application/json; charset=utf-8", json, ParameterType.RequestBody);
            request.RequestFormat = DataFormat.Json;
            lblOutput.Text = "Sending...";


            //Call RxStream
            var restResponse = await client.ExecuteTaskAsync(request);
            lblOutput.Text = "recieved...";

            //try to make the call and report any errors
            try
            {
                //Deserialize object
                var responseList = JsonConvert.DeserializeObject<List<ClosestEstimateDto>>(restResponse.Content);
                gridJson.DataSource = responseList;

                txtJson.Text = restResponse.Content;
            }
            catch (Exception e)
            {
                // txtOutput.Text = e.ToString(); // this really doesn't add value to the error
                lblOutput.Text = restResponse.Content;

                //write it to the text box incase there is a lot of data
                txtJson.Text = restResponse.Content;
            }

            lblOutput.Text = "complete";
        }

    }
}
