﻿document.getElementById('login-passkey-submit').addEventListener('click', handleSignInSubmit);

async function handleSignInSubmit(event) {
    event && event.preventDefault();

    let username = document.forms['passkey']['Passkey.Email'].value;
    let rememberMe = document.forms['passkey']['Passkey.RememberMe'].value;
    let user_verification = "preferred";

    // prepare form post data
    var formData = new FormData();
    formData.append('username', username);
    formData.append('userVerification', user_verification);
    
    // send to server for registering
    let makeAssertionOptions;
    try {
        var res = await fetch('/assertionOptions', {
            method: 'POST', // or 'PUT'
            body: formData, // data can be `string` or {object}!
            headers: {
                'Accept': 'application/json',
                'RequestVerificationToken': document.getElementById('RequestVerificationToken').value
            }
        });

        makeAssertionOptions = await res.json();
    } catch (e) {
        showErrorAlert("Request to server failed", e);
    }

    console.log("Assertion Options Object", makeAssertionOptions);

    // show options error to user
    if (makeAssertionOptions.status !== "ok") {
        console.log("Error creating assertion options");
        console.log(makeAssertionOptions.errorMessage);
        showErrorAlert(makeAssertionOptions.errorMessage);
        return;
    }

    // todo: switch this to coercebase64
    const challenge = makeAssertionOptions.challenge.replace(/-/g, "+").replace(/_/g, "/");
    makeAssertionOptions.challenge = Uint8Array.from(atob(challenge), c => c.charCodeAt(0));

    // fix escaping. Change this to coerce
    makeAssertionOptions.allowCredentials.forEach(function (listItem) {
        var fixedId = listItem.id.replace(/\_/g, "/").replace(/\-/g, "+");
        listItem.id = Uint8Array.from(atob(fixedId), c => c.charCodeAt(0));
    });

    console.log("Assertion options", makeAssertionOptions);

    //const fido2TapKeyToLogin = document.getElementById('fido2TapKeyToLogin').innerText;
    //document.getElementById('fido2logindisplay').innerHTML += '<br><b>' + fido2TapKeyToLogin + '</b><img src = "/images/securitykey.min.svg" alt = "fido login" />';

    // ask browser for credentials (browser will ask connected authenticators)
    let credential;
    try {
        credential = await navigator.credentials.get({ publicKey: makeAssertionOptions });
    } catch (err) {
        //document.getElementById('fido2logindisplay').innerHTML = '';
        showErrorAlert(err.message ? err.message : err);
    }

    //document.getElementById('fido2logindisplay').innerHTML = '<p>Processing</p>';

    try {
        await verifyAssertionWithServer(credential, rememberMe);
    } catch (e) {
        //document.getElementById('fido2logindisplay').innerHTML = '';
        //const fido2CouldNotVerifyAssertion = document.getElementById('fido2CouldNotVerifyAssertion').innerText;
        showErrorAlert(fido2CouldNotVerifyAssertion, e);
    }
}

async function verifyAssertionWithServer(assertedCredential, rememberMe) {
    // Move data into Arrays incase it is super long
    let authData = new Uint8Array(assertedCredential.response.authenticatorData);
    let clientDataJSON = new Uint8Array(assertedCredential.response.clientDataJSON);
    let rawId = new Uint8Array(assertedCredential.rawId);
    let sig = new Uint8Array(assertedCredential.response.signature);
    const data = {
        id: assertedCredential.id,
        rawId: coerceToBase64Url(rawId),
        type: assertedCredential.type,
        extensions: assertedCredential.getClientExtensionResults(),
        response: {
            authenticatorData: coerceToBase64Url(authData),
            clientDataJson: coerceToBase64Url(clientDataJSON),
            signature: coerceToBase64Url(sig)
        }
    };

    let response;
    try {
        let res = await fetch("/makeAssertion", {
            method: 'POST', // or 'PUT'
            body: JSON.stringify(data), // data can be `string` or {object}!
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.getElementById('RequestVerificationToken').value,
                'RememberMe': rememberMe
            }
        });

        response = await res.json();
    } catch (e) {
        showErrorAlert("Request to server failed", e);
        throw e;
    }

    //console.log("Assertion Object", response);

    // show error
    if (response.status !== "ok") {
        console.log("Error doing assertion");
        console.log(response.errorMessage);
        //document.getElementById('fido2logindisplay').innerHTML = '';
        showErrorAlert(response.errorMessage);
        return;
    }

    //document.getElementById('fido2logindisplay').innerHTML = '<p>Logged In!</p>';

    let fido2ReturnUrl = document.getElementById('passkeysReturnUrl').innerText;
    if (!fido2ReturnUrl) {
        fido2ReturnUrl = "/";
    }
    window.location.href = fido2ReturnUrl;
}
