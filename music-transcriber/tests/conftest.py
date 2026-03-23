"""
conftest.py — shared pytest fixtures available to all test modules.
"""

import pytest


@pytest.fixture(scope="session")
def session_tmp(tmp_path_factory):
    """Session-scoped temp dir for artefacts that are expensive to recreate."""
    return tmp_path_factory.mktemp("session")
