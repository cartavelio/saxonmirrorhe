# SaxonC Python samples

There are a number of samples provided to show you some of the basics in context.

Once you've installed the SaxonC Python wheel:

    $ pip install saxonc@edition@

You can run the samples from inside their directory:

    $ cd samples
    $ python saxon_example2.py

Also provided are the `pytest`-run tests we run ourselves. Because these are
run against every edition (HE, PE, EE) of SaxonC, they're a little more involved 
to run. You need to tell the runner which edition you're using by setting the 
`SAXONC_EDITION` environment variable:

    $ pip install pytest
    $ SAXONC_EDITION=@edition@ pytest --import-mode=importlib \
        --data-dir=../samples/data \
        test 

Or, with PowerShell on Windows:

    PS > pip install pytest
    PS > $env:SAXONC_EDITION = "@edition@"
    PS > pytest --import-mode=importlib --data-dir=../samples/data test 
