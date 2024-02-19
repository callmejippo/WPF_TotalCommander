    package com.example.lab_week2;

import androidx.appcompat.app.AppCompatActivity;

import android.app.Activity;
import android.content.SharedPreferences;
import android.graphics.Color;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.TextView;
import android.widget.Toast;

import java.util.Locale;

    public class MainActivity extends AppCompatActivity {

        TextView txtSpyBox;
        LinearLayout myScreen;
        EditText txtColorSelected;
        EditText txtMsg;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        Toast.makeText(this, "onCreate", Toast.LENGTH_SHORT ).show();
        txtMsg = findViewById(R.id.editText1);
        txtColorSelected = (EditText)findViewById(R.id.editText1);
        txtSpyBox = (TextView)findViewById(R.id.textView1);
        myScreen =(LinearLayout)findViewById(R.id.myScreen1);
        Button btnExit = (Button)findViewById(R.id.button1);
        btnExit.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                finish();
            }
        });

        txtColorSelected.addTextChangedListener(new TextWatcher() {
            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) { /* nothing TODO, needed by interface */ }
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) { /* nothing TODO, needed by interface */ }
            @Override
            public void afterTextChanged(Editable s) {
//set background to selected color
                String chosenColor = s.toString().toLowerCase(Locale.US);
                txtSpyBox.setText(chosenColor);
                stBgColor(chosenColor, myScreen);
            }
        });
    }
    @Override
    protected void onStart(){
        super.onStart();
        updateMeUsingSavedStateData();
        Toast.makeText(this, "onStart", Toast.LENGTH_SHORT ).show();
    }
    @Override
    protected void onRestart(){
        super.onRestart();
        Toast.makeText(this, "onRestart", Toast.LENGTH_SHORT ).show();
    }

    @Override
    protected void onResume(){
        super.onResume();
        Toast.makeText(this, "onResume", Toast.LENGTH_SHORT ).show();
        SharedPreferences myFile = getSharedPreferences("myFile1", Activity.MODE_PRIVATE);
        if ( (myFile != null) && (myFile.contains("mydata")) ) {
            String temp = myFile.getString("mydata", "***");
            txtMsg.setText(temp);
        }
    }


    @Override
    protected void onPause() {
        super.onPause();
        Toast.makeText(this, "onPause", Toast.LENGTH_SHORT ).show();
        String chosenColor = txtSpyBox.getText().toString();
        saveStateData(chosenColor);
    }

    @Override
    protected void onStop() {
        super.onStop();
        Toast.makeText(this, "onStop", Toast.LENGTH_SHORT).show();
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        Toast.makeText(this, "onDestroy", Toast.LENGTH_SHORT ).show();

    }

    private void stBgColor(String chosenColor, LinearLayout myScreen)
    {
        if(chosenColor.contains("red")) myScreen.setBackgroundColor(0xffff0000);
        if(chosenColor.contains("green")) myScreen.setBackgroundColor(0xff00ff00);
        if(chosenColor.contains("blue")) myScreen.setBackgroundColor(0xff0000ff);
        if(chosenColor.contains("white")) myScreen.setBackgroundColor(0xffffffff);

    }
    private void saveStateData(String chosenColor) {
    //this is a little <key,value> table permanently kept in memory
        SharedPreferences myPrefContainer = getSharedPreferences("myFile1", Activity.MODE_PRIVATE);
    //pair <key,value> to be stored represents our 'important' data
        SharedPreferences.Editor myPrefEditor = myPrefContainer.edit();
        String key = "chosenBackgroundColor", value = txtSpyBox.getText().toString();
        myPrefEditor.putString(key, value);
        myPrefEditor.apply();
    }//saveStateData

        private void updateMeUsingSavedStateData() {
// (in case it exists) use saved data telling backg color
            SharedPreferences myPrefContainer = getSharedPreferences("myFile1", Activity.MODE_PRIVATE);
            String key = "chosenBackgroundColor";
            String defaultValue = "white";
            if (( myPrefContainer != null ) && myPrefContainer.contains(key)){
                String color = myPrefContainer.getString(key, defaultValue);
                stBgColor(color, myScreen);
            }
        }//updateMeUsingSavedStateData
}