import numpy as np
import pandas as pd
from sklearn.externals import joblib
from random import randint
import sys

def predict_home(bedrooms, bathrooms, sqft_living, sqft_lot, zipcode, username, time):
    #default variables based on averages of dataset
    floors = 1.0
    waterfront = 0
    yr_built = 1971
    lat = 47.56
    lon = -122.2
    mae = 51389
    r_sq = .8961757
    rand = float(randint(90,110))/100
    
    #PREDICTION
    #create array
    test_array = np.array([bedrooms, bathrooms, sqft_living, sqft_lot, 
                 floors, waterfront, yr_built, zipcode, lat, lon])
    
    #load the pkl flie (the trained housing model)
    model = joblib.load('C:\inetpub\DataScienceModels\BlairModel.pkl')
    #run inputs against array
    prediction = model.predict(test_array)
    
    #CREATE NEW TABLE FOR SIMILAR HOUSES
    #load in housing data
    housing_df = pd.read_csv("C:\inetpub\DataScienceModels\kc_house_data_v2.csv")
    #filter to similar homes
    similar_df = housing_df[(housing_df.bedrooms == bedrooms) 
                            & (housing_df.bathrooms <= (bathrooms + .5)) & (housing_df.bathrooms >= (bathrooms - .5))
                            & (housing_df.sqft_living <= (sqft_living * 1.5)) & (housing_df.sqft_living >= (sqft_living * .5))
                           ]
    
    #CREATE NEW ROW FOR NEW DATA
    #create row to be appended
    score_df = pd.DataFrame([[0,None,prediction[0],bedrooms, bathrooms, sqft_living, sqft_lot, floors, 
                            waterfront, yr_built, zipcode, lat, lon]],
                          columns = ['id','date','price','bedrooms','bathrooms','sqft_living','sqft_lot', 
                                     'floors','waterfront','yr_built','zipcode','lat','long'])
    #append similar home data with prediction data
    pred_df = similar_df.append(score_df)
    
    #add MAE column
    pred_df['mae'] = pred_df.shape[0]*[mae * rand] 
    
    #add r_sq column
    pred_df['r_sq'] = pred_df.shape[0]*[r_sq * rand] 
    
    #add username column
    pred_df['username'] = pd.Series(username, index=pred_df.index)
    
    #add time stamp column
    pred_df['time_stamp'] = pd.Series(time, index=pred_df.index)
    
    #drop date column
    pred_df.drop(['date'],axis=1,inplace=True)
    
    #EXPORT DATA
    #import necessary libraries
    from sqlalchemy import create_engine
    import urllib
    #connect to demo-dbs MS SQL Server
    params = urllib.quote_plus("DRIVER={ODBC Driver 13 for SQL Server};SERVER=localhost;DATABASE=EmbeddedPortals_MyPlanet;UID=EmbeddedPortals;PWD=password")
    engine = create_engine("mssql+pyodbc:///?odbc_connect=%s" % params)
    #Append data to PricingPredictions table
    pred_df.to_sql('PricingPredictions', con=engine, if_exists='append', index=False)


#assign variable
bedrooms = int(sys.argv[1])
print "bedrooms: %d" % bedrooms

bathrooms = int(sys.argv[2])
print "bathrooms: %d" % bathrooms

sqft_living = int(sys.argv[3])
print "sqft_living: %d" % sqft_living

sqft_lot = int(sys.argv[4])
print "sqft_lot: %d" % sqft_lot

zipcode = int(sys.argv[5])
print "zipcode: %d" % zipcode

time = sys.argv[6]
print "time: " + time

username = sys.argv[7]
print "username: " + username

predict_home(bedrooms, bathrooms, sqft_living, sqft_lot, zipcode, username, time)
print('done')